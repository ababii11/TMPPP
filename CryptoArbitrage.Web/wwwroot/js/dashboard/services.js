import { apiClient } from "./apiClient.js";

export const walletService = {
  getBalance() {
    return apiClient.request("wallet-balance", "/api/wallet/balance");
  },

  postTransaction(payload) {
    return apiClient.request("wallet-transaction", "/api/wallet/transaction", {
      method: "POST",
      body: payload
    });
  },

  getHistory() {
    return apiClient.request("wallet-history", "/api/wallet/history");
  }
};

export const cryptoService = {
  getPrices() {
    return apiClient.request("crypto-prices", "/api/crypto/prices");
  },

  getLivePrices(symbol, strategy) {
    const safeSymbol = encodeURIComponent(symbol || "BTC");
    const safeStrategy = encodeURIComponent(strategy || "simple");
    return apiClient.request(
      "crypto-prices-live",
      `/api/crypto/prices-live?symbol=${safeSymbol}&strategy=${safeStrategy}&feePercent=0.1&minProfitPercent=0.5&spreadAlertPercent=0.2`
    );
  }
};

export const tradeService = {
  executeCommand(payload) {
    return apiClient.request("trade-command-execute", "/api/trade/execute", {
      method: "POST",
      body: payload
    });
  },

  undoLastCommand() {
    return apiClient.request("trade-command-undo", "/api/trade/undo", {
      method: "POST"
    });
  }
};

export const botStateService = {
  getCurrent() {
    return apiClient.request("bot-state-current", "/api/botstate/current");
  },

  getSnapshots() {
    return apiClient.request("bot-state-snapshots", "/api/botstate/snapshots");
  },

  saveSnapshot(label) {
    return apiClient.request("bot-state-save", "/api/botstate/save", {
      method: "POST",
      body: { label }
    });
  },

  restoreSnapshot(snapshotId) {
    return apiClient.request("bot-state-restore", `/api/botstate/restore/${encodeURIComponent(snapshotId)}`, {
      method: "POST"
    });
  }
};
