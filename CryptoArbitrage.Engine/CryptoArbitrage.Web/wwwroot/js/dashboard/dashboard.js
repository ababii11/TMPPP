import { walletService, cryptoService, tradeService, botStateService } from "./services.js";
import { setCardLoading, renderPriceCards, renderHistoryRows, showToast, formatMoney, drawPriceChart } from "./ui.js";

const ESTIMATED_FEE_PERCENT = 0.2;

const refs = {
  balanceCard: document.getElementById("balanceCard"),
  pricesCard: document.getElementById("pricesCard"),
  transactionCard: document.getElementById("transactionCard"),
  commandCard: document.getElementById("commandCard"),
  historyCard: document.getElementById("historyCard"),
  balanceValue: document.getElementById("walletBalanceValue"),
  balanceMeta: document.getElementById("walletBalanceMeta"),
  pricesList: document.getElementById("pricesList"),
  pricesChart: document.getElementById("pricesChart"),
  strategyType: document.getElementById("strategyType"),
  liveSymbol: document.getElementById("liveSymbol"),
  observerEvents: document.getElementById("observerEvents"),
  transactionForm: document.getElementById("transactionForm"),
  fromAddress: document.getElementById("fromAddress"),
  toAddress: document.getElementById("toAddress"),
  amount: document.getElementById("amount"),
  cryptoType: document.getElementById("cryptoType"),
  feePreview: document.getElementById("feePreview"),
  netAmountPreview: document.getElementById("netAmountPreview"),
  metaPreview: document.getElementById("metaPreview"),
  txFormError: document.getElementById("txFormError"),
  tradeCommandForm: document.getElementById("tradeCommandForm"),
  tradeExchange: document.getElementById("tradeExchange"),
  tradeSide: document.getElementById("tradeSide"),
  tradePair: document.getElementById("tradePair"),
  tradeAmount: document.getElementById("tradeAmount"),
  tradePrice: document.getElementById("tradePrice"),
  undoTradeCommandBtn: document.getElementById("undoTradeCommandBtn"),
  tradeCommandError: document.getElementById("tradeCommandError"),
  tradeCommandResult: document.getElementById("tradeCommandResult"),
  tradeCommandHistory: document.getElementById("tradeCommandHistory"),
  snapshotLabel: document.getElementById("snapshotLabel"),
  snapshotSelect: document.getElementById("snapshotSelect"),
  saveBotStateBtn: document.getElementById("saveBotStateBtn"),
  restoreBotStateBtn: document.getElementById("restoreBotStateBtn"),
  persistenceStatus: document.getElementById("persistenceStatus"),
  botActiveStrategy: document.getElementById("botActiveStrategy"),
  historyTableBody: document.getElementById("historyTableBody"),
  toastStack: document.getElementById("toastStack")
};

const state = {
  prices: [],
  history: [],
  observerEvents: []
};

function normalizeBalance(response) {
  return {
    amount: Number(response?.balance ?? response?.totalBalance ?? response?.amount ?? 0),
    currency: response?.currency || "USDT",
    accountName: response?.walletName || response?.account || "Primary wallet"
  };
}

function normalizePrices(response) {
  if (Array.isArray(response)) {
    return response.map((item) => ({
      symbol: String(item.symbol || item.cryptoType || item.name || "N/A").toUpperCase(),
      price: Number(item.price ?? item.currentPrice ?? 0)
    }));
  }

  if (response && typeof response === "object") {
    return Object.entries(response).map(([symbol, price]) => ({
      symbol: symbol.toUpperCase(),
      price: Number(price || 0)
    }));
  }

  return [];
}

function normalizeHistory(response) {
  const rows = Array.isArray(response) ? response : response?.items || response?.history || [];

  return rows.map((item) => ({
    timestamp: item.timestamp || item.date || item.createdAt || "-",
    from: item.from || item.source || "-",
    to: item.to || item.destination || "-",
    cryptoType: item.cryptoType || item.symbol || "-",
    amount: Number(item.amount || 0),
    status: item.status || "Pending"
  }));
}

function updateTransactionDecorator() {
  const amount = Number(refs.amount.value || 0);
  const fee = amount * (ESTIMATED_FEE_PERCENT / 100);
  const net = amount - fee;

  refs.feePreview.textContent = `${formatMoney(fee, 6)} ${refs.cryptoType.value || ""}`;
  refs.netAmountPreview.textContent = `${formatMoney(net, 6)} ${refs.cryptoType.value || ""}`;
  refs.metaPreview.textContent = `fee:${ESTIMATED_FEE_PERCENT.toFixed(2)}% | source:web-client`;
}

function populateCryptoTypes(prices) {
  const fallback = ["BTC", "ETH", "SOL", "ADA"];
  const available = [...new Set([...prices.map((p) => p.symbol), ...fallback])];

  refs.cryptoType.innerHTML = "";
  available.forEach((symbol) => {
    const option = document.createElement("option");
    option.value = symbol;
    option.textContent = symbol;
    refs.cryptoType.appendChild(option);
  });

  updateTransactionDecorator();
  populateLiveSymbols(available);
}

function populateLiveSymbols(symbols) {
  if (!refs.liveSymbol) {
    return;
  }

  const current = refs.liveSymbol.value;
  refs.liveSymbol.innerHTML = "";

  symbols.forEach((symbol) => {
    const option = document.createElement("option");
    option.value = symbol;
    option.textContent = symbol;
    refs.liveSymbol.appendChild(option);
  });

  if (current && symbols.includes(current)) {
    refs.liveSymbol.value = current;
  }
}

function renderObserverEvents(events) {
  if (!refs.observerEvents) {
    return;
  }

  refs.observerEvents.innerHTML = "";
  if (!events.length) {
    const item = document.createElement("li");
    item.textContent = "No observer events yet.";
    refs.observerEvents.appendChild(item);
    return;
  }

  events.slice(0, 6).forEach((entry) => {
    const item = document.createElement("li");
    item.textContent = entry;
    refs.observerEvents.appendChild(item);
  });
}

async function loadBalance() {
  setCardLoading(refs.balanceCard, true);
  try {
    const response = await walletService.getBalance();
    const wallet = normalizeBalance(response);
    refs.balanceValue.textContent = `$${formatMoney(wallet.amount, 2)}`;
    refs.balanceMeta.textContent = `${wallet.accountName} | ${wallet.currency}`;
  } catch (error) {
    refs.balanceValue.textContent = "Unavailable";
    refs.balanceMeta.textContent = "Could not read wallet balance.";
    showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.balanceCard, false);
  }
}

async function loadPrices() {
  setCardLoading(refs.pricesCard, true);
  try {
    const strategy = refs.strategyType?.value || "simple";
    const symbol = refs.liveSymbol?.value || "BTC";
    const response = await cryptoService.getLivePrices(symbol, strategy);

    state.prices = normalizePrices(response?.prices || []);
    state.observerEvents = Array.isArray(response?.observerEvents) ? response.observerEvents : [];

    renderPriceCards(refs.pricesList, state.prices);
    populateCryptoTypes(state.prices);
    drawPriceChart(refs.pricesChart, state.prices);
    renderObserverEvents(state.observerEvents);
    refs.botActiveStrategy.textContent = response?.strategy || refs.botActiveStrategy.textContent;

    if (response?.opportunity?.isProfitable) {
      showToast(
        refs.toastStack,
        `Opportunity ${response.symbol}: ${response.opportunity.exchangeBuy} -> ${response.opportunity.exchangeSell}`,
        "success"
      );
    }
  } catch (error) {
    renderPriceCards(refs.pricesList, []);
    renderObserverEvents([]);
    showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.pricesCard, false);
  }
}

async function loadHistory() {
  setCardLoading(refs.historyCard, true);
  try {
    const response = await walletService.getHistory();
    state.history = normalizeHistory(response);
    renderHistoryRows(refs.historyTableBody, state.history);
  } catch (error) {
    renderHistoryRows(refs.historyTableBody, []);
    showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.historyCard, false);
  }
}

function getFormPayload() {
  return {
    from: refs.fromAddress.value.trim(),
    to: refs.toAddress.value.trim(),
    amount: Number(refs.amount.value),
    cryptoType: refs.cryptoType.value
  };
}

function validateForm(payload) {
  if (!payload.from || !payload.to || !payload.cryptoType) {
    return "All fields are required.";
  }

  if (!payload.amount || Number.isNaN(payload.amount) || payload.amount <= 0) {
    return "Amount must be greater than zero.";
  }

  return "";
}

async function submitTransaction(event) {
  event.preventDefault();
  refs.txFormError.hidden = true;

  const payload = getFormPayload();
  const validationError = validateForm(payload);
  if (validationError) {
    refs.txFormError.textContent = validationError;
    refs.txFormError.hidden = false;
    return;
  }

  setCardLoading(refs.transactionCard, true);

  try {
    await walletService.postTransaction(payload);
    showToast(refs.toastStack, "Transaction completed.", "success");
    refs.transactionForm.reset();
    populateCryptoTypes(state.prices);
    await Promise.all([loadBalance(), loadHistory()]);
  } catch (error) {
    refs.txFormError.textContent = error.message;
    refs.txFormError.hidden = false;
    showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.transactionCard, false);
  }
}

function getTradeCommandPayload() {
  return {
    exchange: refs.tradeExchange?.value || "binance",
    side: refs.tradeSide?.value || "buy",
    pair: refs.tradePair?.value?.trim() || "BTC/USDT",
    amount: Number(refs.tradeAmount?.value),
    price: Number(refs.tradePrice?.value)
  };
}

async function submitTradeCommand(event) {
  event.preventDefault();

  if (!refs.tradeCommandForm) {
    return;
  }

  refs.tradeCommandError.hidden = true;
  const payload = getTradeCommandPayload();

  if (!payload.pair || !payload.amount || payload.amount <= 0 || !payload.price || payload.price <= 0) {
    refs.tradeCommandError.textContent = "Invalid trade command payload.";
    refs.tradeCommandError.hidden = false;
    return;
  }

  setCardLoading(refs.commandCard, true);
  try {
    const response = await tradeService.executeCommand(payload);
    refs.tradeCommandResult.textContent = response.execution || "Executed";
    refs.tradeCommandHistory.textContent = String(response.historyCount ?? 0);
    showToast(refs.toastStack, `${response.command} executed on ${response.exchange}.`, "success");
    await refreshBotStatePanels();
  } catch (error) {
    refs.tradeCommandError.textContent = error.message;
    refs.tradeCommandError.hidden = false;
    showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.commandCard, false);
  }
}

async function undoTradeCommand() {
  if (!refs.undoTradeCommandBtn) {
    return;
  }

  refs.tradeCommandError.hidden = true;
  setCardLoading(refs.commandCard, true);
  try {
    const response = await tradeService.undoLastCommand();
    refs.tradeCommandResult.textContent = response.undoResult || "Undone";
    refs.tradeCommandHistory.textContent = String(response.historyCount ?? 0);
    showToast(refs.toastStack, "Last command undone.", "success");
    await refreshBotStatePanels();
  } catch (error) {
    refs.tradeCommandError.textContent = error.message;
    refs.tradeCommandError.hidden = false;
    showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.commandCard, false);
  }
}

async function refreshSnapshots() {
  if (!refs.snapshotSelect) {
    return;
  }

  const snapshots = await botStateService.getSnapshots();
  const current = refs.snapshotSelect.value;
  refs.snapshotSelect.innerHTML = "";

  if (!Array.isArray(snapshots) || snapshots.length === 0) {
    const option = document.createElement("option");
    option.value = "";
    option.textContent = "No snapshots";
    refs.snapshotSelect.appendChild(option);
    if (refs.persistenceStatus) {
      refs.persistenceStatus.textContent = "SQL Server persistence is active, but no snapshots are saved yet.";
    }
    return;
  }

  snapshots.forEach((snapshot) => {
    const option = document.createElement("option");
    option.value = snapshot.snapshotId;
    option.textContent = `${snapshot.label} | ${snapshot.capturedAt}`;
    refs.snapshotSelect.appendChild(option);
  });

  if (current && snapshots.some((s) => s.snapshotId === current)) {
    refs.snapshotSelect.value = current;
  }

  if (refs.persistenceStatus) {
    const latest = snapshots[0];
    refs.persistenceStatus.textContent = `SQL Server persistence is active | ${snapshots.length} snapshot(s) stored | latest: ${latest.label}`;
  }
}

async function refreshBotStatePanels() {
  const current = await botStateService.getCurrent();
  refs.botActiveStrategy.textContent = current?.activeStrategy || "-";
  await refreshSnapshots();
}

async function saveBotState() {
  setCardLoading(refs.commandCard, true);
  refs.tradeCommandError.hidden = true;
  try {
    const label = refs.snapshotLabel?.value?.trim() || "manual-snapshot";
    const response = await botStateService.saveSnapshot(label);
    refs.tradeCommandResult.textContent = `Saved snapshot ${response.snapshotId}`;
    showToast(refs.toastStack, "Bot state snapshot saved.", "success");
    await refreshBotStatePanels();
  } catch (error) {
    refs.tradeCommandError.textContent = error.message;
    refs.tradeCommandError.hidden = false;
    showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.commandCard, false);
  }
}

async function restoreBotState() {
  const snapshotId = refs.snapshotSelect?.value;
  if (!snapshotId) {
    refs.tradeCommandError.textContent = "Select a snapshot to restore.";
    refs.tradeCommandError.hidden = false;
    return;
  }

  setCardLoading(refs.commandCard, true);
  refs.tradeCommandError.hidden = true;
  try {
    const response = await botStateService.restoreSnapshot(snapshotId);
    refs.tradeCommandResult.textContent = response.message || "Restored";
    refs.tradeCommandHistory.textContent = "0";
    await Promise.all([loadBalance(), loadHistory(), loadPrices()]);
    await refreshBotStatePanels();
    showToast(refs.toastStack, "Bot state restored from snapshot.", "success");
  } catch (error) {
    refs.tradeCommandError.textContent = error.message;
    refs.tradeCommandError.hidden = false;
    showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.commandCard, false);
  }
}

function wireEvents() {
  refs.transactionForm.addEventListener("submit", submitTransaction);
  refs.amount.addEventListener("input", updateTransactionDecorator);
  refs.cryptoType.addEventListener("change", updateTransactionDecorator);
  refs.strategyType?.addEventListener("change", loadPrices);
  refs.liveSymbol?.addEventListener("change", loadPrices);
  refs.tradeCommandForm?.addEventListener("submit", submitTradeCommand);
  refs.undoTradeCommandBtn?.addEventListener("click", undoTradeCommand);
  refs.saveBotStateBtn?.addEventListener("click", saveBotState);
  refs.restoreBotStateBtn?.addEventListener("click", restoreBotState);
}

async function initializeDashboard() {
  wireEvents();
  populateCryptoTypes([]);
  await Promise.all([loadBalance(), loadPrices(), loadHistory()]);
  await refreshBotStatePanels();
}

initializeDashboard();
