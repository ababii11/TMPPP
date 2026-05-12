import { walletService, cryptoService, tradeService, botStateService } from "./services.js";
import { setCardLoading, renderPriceCards, renderHistoryRows, showToast, formatMoney, drawPriceChart } from "./ui.js";

const ESTIMATED_FEE_PERCENT = 0.2;

// Safe element getter - returns null if element doesn't exist
const getEl = (id) => document.getElementById(id);

const refs = {
  balanceCard: getEl("balanceCard"),
  pricesCard: getEl("pricesCard"),
  transactionCard: getEl("transactionCard"),
  commandCard: getEl("commandCard"),
  historyCard: getEl("historyCard"),
  balanceValue: getEl("walletBalanceValue"),
  balanceMeta: getEl("walletBalanceMeta"),
  pricesList: getEl("pricesList"),
  pricesChart: getEl("pricesChart"),
  strategyType: getEl("strategyType"),
  liveSymbol: getEl("liveSymbol"),
  observerEvents: getEl("observerEvents"),
  transactionForm: getEl("transactionForm"),
  fromAddress: getEl("fromAddress"),
  toAddress: getEl("toAddress"),
  amount: getEl("amount"),
  cryptoType: getEl("cryptoType"),
  feePreview: getEl("feePreview"),
  netAmountPreview: getEl("netAmountPreview"),
  metaPreview: getEl("metaPreview"),
  txFormError: getEl("txFormError"),
  tradeCommandForm: getEl("tradeCommandForm"),
  tradeExchange: getEl("tradeExchange"),
  tradeSide: getEl("tradeSide"),
  tradePair: getEl("tradePair"),
  tradeAmount: getEl("tradeAmount"),
  tradePrice: getEl("tradePrice"),
  undoTradeCommandBtn: getEl("undoTradeCommandBtn"),
  tradeCommandError: getEl("tradeCommandError"),
  tradeCommandResult: getEl("tradeCommandResult"),
  tradeCommandHistory: getEl("tradeCommandHistory"),
  snapshotLabel: getEl("snapshotLabel"),
  snapshotSelect: getEl("snapshotSelect"),
  saveBotStateBtn: getEl("saveBotStateBtn"),
  restoreBotStateBtn: getEl("restoreBotStateBtn"),
  persistenceStatus: getEl("persistenceStatus"),
  botActiveStrategy: getEl("botActiveStrategy"),
  historyTableBody: getEl("historyTableBody"),
  toastStack: getEl("toastStack")
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
  if (!refs.amount || !refs.feePreview || !refs.netAmountPreview || !refs.metaPreview) {
    return;
  }
  
  const amount = Number(refs.amount.value || 0);
  const fee = amount * (ESTIMATED_FEE_PERCENT / 100);
  const net = amount - fee;

  refs.feePreview.textContent = `${formatMoney(fee, 6)} ${refs.cryptoType?.value || ""}`;
  refs.netAmountPreview.textContent = `${formatMoney(net, 6)} ${refs.cryptoType?.value || ""}`;
  refs.metaPreview.textContent = `fee:${ESTIMATED_FEE_PERCENT.toFixed(2)}% | source:web-client`;
}

function populateCryptoTypes(prices) {
  if (!refs.cryptoType) {
    return;
  }
  
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
  if (!refs.balanceCard) {
    return;
  }
  
  setCardLoading(refs.balanceCard, true);
  try {
    const response = await walletService.getBalance();
    const wallet = normalizeBalance(response);
    if (refs.balanceValue) refs.balanceValue.textContent = `$${formatMoney(wallet.amount, 2)}`;
    if (refs.balanceMeta) refs.balanceMeta.textContent = `${wallet.accountName} | ${wallet.currency}`;
  } catch (error) {
    if (refs.balanceValue) refs.balanceValue.textContent = "Unavailable";
    if (refs.balanceMeta) refs.balanceMeta.textContent = "Could not read wallet balance.";
    if (refs.toastStack) showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.balanceCard, false);
  }
}

async function loadPrices() {
  if (!refs.pricesCard) {
    return;
  }
  
  setCardLoading(refs.pricesCard, true);
  try {
    const strategy = refs.strategyType?.value || "simple";
    const symbol = refs.liveSymbol?.value || "BTC";
    const response = await cryptoService.getLivePrices(symbol, strategy);

    state.prices = normalizePrices(response?.prices || []);
    state.observerEvents = Array.isArray(response?.observerEvents) ? response.observerEvents : [];

    if (refs.pricesList) renderPriceCards(refs.pricesList, state.prices);
    populateCryptoTypes(state.prices);
    if (refs.pricesChart) drawPriceChart(refs.pricesChart, state.prices);
    renderObserverEvents(state.observerEvents);
    if (refs.botActiveStrategy) refs.botActiveStrategy.textContent = response?.strategy || refs.botActiveStrategy.textContent;

    if (response?.opportunity?.isProfitable && refs.toastStack) {
      showToast(
        refs.toastStack,
        `Opportunity ${response.symbol}: ${response.opportunity.exchangeBuy} -> ${response.opportunity.exchangeSell}`,
        "success"
      );
    }
  } catch (error) {
    if (refs.pricesList) renderPriceCards(refs.pricesList, []);
    renderObserverEvents([]);
    if (refs.toastStack) showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.pricesCard, false);
  }
}

async function loadHistory() {
  if (!refs.historyCard) {
    return;
  }
  
  setCardLoading(refs.historyCard, true);
  try {
    const response = await walletService.getHistory();
    state.history = normalizeHistory(response);
    if (refs.historyTableBody) renderHistoryRows(refs.historyTableBody, state.history);
  } catch (error) {
    if (refs.historyTableBody) renderHistoryRows(refs.historyTableBody, []);
    if (refs.toastStack) showToast(refs.toastStack, error.message, "error");
  } finally {
    setCardLoading(refs.historyCard, false);
  }
}

function getFormPayload() {
  if (!refs.fromAddress || !refs.toAddress || !refs.amount || !refs.cryptoType) {
    return null;
  }
  
  return {
    from: refs.fromAddress.value.trim(),
    to: refs.toAddress.value.trim(),
    amount: Number(refs.amount.value),
    cryptoType: refs.cryptoType.value
  };
}

function validateForm(payload) {
  if (!payload || !payload.from || !payload.to || !payload.cryptoType) {
    return "All fields are required.";
  }

  if (!payload.amount || Number.isNaN(payload.amount) || payload.amount <= 0) {
    return "Amount must be greater than zero.";
  }

  return "";
}

async function submitTransaction(event) {
  if (!refs.transactionForm) {
    return;
  }
  
  event.preventDefault();
  if (refs.txFormError) refs.txFormError.hidden = true;

  const payload = getFormPayload();
  if (!payload) {
    if (refs.txFormError) {
      refs.txFormError.textContent = "Form elements not found.";
      refs.txFormError.hidden = false;
    }
    return;
  }
  
  const validationError = validateForm(payload);
  if (validationError) {
    if (refs.txFormError) {
      refs.txFormError.textContent = validationError;
      refs.txFormError.hidden = false;
    }
    return;
  }

  setCardLoading(refs.transactionCard, true);

  try {
    await walletService.postTransaction(payload);
    if (refs.toastStack) showToast(refs.toastStack, "Transaction completed.", "success");
    if (refs.transactionForm) refs.transactionForm.reset();
    populateCryptoTypes(state.prices);
    await Promise.all([loadBalance(), loadHistory()]);
  } catch (error) {
    if (refs.txFormError) {
      refs.txFormError.textContent = error.message;
      refs.txFormError.hidden = false;
    }
    if (refs.toastStack) showToast(refs.toastStack, error.message, "error");
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
  // Only wire events for elements that exist on this page
  if (refs.transactionForm) refs.transactionForm.addEventListener("submit", submitTransaction);
  if (refs.amount) refs.amount.addEventListener("input", updateTransactionDecorator);
  if (refs.cryptoType) refs.cryptoType.addEventListener("change", updateTransactionDecorator);
  if (refs.strategyType) refs.strategyType.addEventListener("change", loadPrices);
  if (refs.liveSymbol) refs.liveSymbol.addEventListener("change", loadPrices);
  if (refs.tradeCommandForm) refs.tradeCommandForm.addEventListener("submit", submitTradeCommand);
  if (refs.undoTradeCommandBtn) refs.undoTradeCommandBtn.addEventListener("click", undoTradeCommand);
  if (refs.saveBotStateBtn) refs.saveBotStateBtn.addEventListener("click", saveBotState);
  if (refs.restoreBotStateBtn) refs.restoreBotStateBtn.addEventListener("click", restoreBotState);
}

async function initializeDashboard() {
  wireEvents();
  // Only load data if the relevant elements exist
  if (refs.cryptoType) populateCryptoTypes([]);
  
  const loadTasks = [];
  if (refs.balanceValue) loadTasks.push(loadBalance());
  if (refs.pricesChart) loadTasks.push(loadPrices());
  if (refs.historyTableBody) loadTasks.push(loadHistory());
  
  if (loadTasks.length > 0) {
    await Promise.all(loadTasks);
  }
  
  if (refs.persistenceStatus) {
    await refreshBotStatePanels();
  }
}

// Only initialize if we're on a page with dashboard elements
if (refs.toastStack || refs.pricesChart || refs.balanceValue) {
  initializeDashboard();
}
