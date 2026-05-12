export function setCardLoading(card, isLoading) {
  const existingLoader = card.querySelector(".loader");

  if (isLoading) {
    card.classList.add("is-loading");
    if (!existingLoader) {
      const loader = document.createElement("div");
      loader.className = "loader";
      loader.setAttribute("aria-label", "Loading");
      card.appendChild(loader);
    }
    return;
  }

  card.classList.remove("is-loading");
  if (existingLoader) {
    existingLoader.remove();
  }
}

export function formatMoney(value, fractionDigits = 2) {
  const safeValue = Number(value || 0);
  return new Intl.NumberFormat("en-US", {
    minimumFractionDigits: fractionDigits,
    maximumFractionDigits: fractionDigits
  }).format(safeValue);
}

function escapeHtml(str) {
  return String(str ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

const STRATEGY_LABELS = {
  simple: "Simple arbitrage",
  spread: "Spread-based",
  triangular: "Triangular route"
};

function humanizeStrategyName(strategy) {
  const s = String(strategy || "").toLowerCase();
  if (s.includes("triangular")) return STRATEGY_LABELS.triangular;
  if (s.includes("spread")) return STRATEGY_LABELS.spread;
  if (s.includes("simple")) return STRATEGY_LABELS.simple;
  return strategy || "Strategy";
}

const CHART_PALETTE = ["#5dc1ff", "#ffba56", "#44e0c4", "#ff6f86", "#c79cff"];

export function renderPriceCards(container, prices) {
  container.innerHTML = "";

  if (!prices.length) {
    container.innerHTML = '<p class="empty-message">No price data available.</p>';
    return;
  }

  prices.forEach((item) => {
    const card = document.createElement("article");
    card.className = "crypto-price-item";
    const ex = item.exchange
      ? `<p class="crypto-exchange">${escapeHtml(item.exchange)}</p>`
      : "";

    card.innerHTML = `
      <h3>${escapeHtml(item.symbol)}</h3>
      ${ex}
      <p>$${formatMoney(item.price, 2)}</p>
    `;

    container.appendChild(card);
  });
}

export function renderHistoryRows(tableBody, rows) {
  tableBody.innerHTML = "";

  if (!rows.length) {
    tableBody.innerHTML = '<tr><td colspan="6" class="empty-message">No transactions yet.</td></tr>';
    return;
  }

  rows.forEach((item) => {
    const row = document.createElement("tr");
    const state = (item.status || "pending").toLowerCase();
    const badgeClass = state === "success" || state === "completed" ? "ok" : state === "failed" ? "failed" : "pending";

    row.innerHTML = `
      <td>${item.timestamp || "-"}</td>
      <td>${item.from || "-"}</td>
      <td>${item.to || "-"}</td>
      <td>${item.cryptoType || item.symbol || "-"}</td>
      <td>${formatMoney(item.amount, 4)}</td>
      <td><span class="badge-status ${badgeClass}">${item.status || "Pending"}</span></td>
    `;

    tableBody.appendChild(row);
  });
}

export function showToast(stack, message, type = "success") {
  const item = document.createElement("div");
  item.className = `toast-item ${type}`;
  item.textContent = message;
  stack.appendChild(item);

  setTimeout(() => {
    item.remove();
  }, 3200);
}

export function drawPriceChart(canvas, prices) {
  const ctx = canvas.getContext("2d");
  if (!ctx || prices.length < 2) {
    return;
  }

  const width = canvas.clientWidth;
  const height = canvas.clientHeight;
  canvas.width = width;
  canvas.height = height;

  const values = prices.map((item) => Number(item.price || 0));
  const min = Math.min(...values);
  const max = Math.max(...values);
  const spread = Math.max(max - min, 1);

  ctx.clearRect(0, 0, width, height);

  ctx.strokeStyle = "rgba(93, 193, 255, 0.22)";
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(0, height - 1);
  ctx.lineTo(width, height - 1);
  ctx.stroke();

  ctx.beginPath();
  values.forEach((value, index) => {
    const x = (index / (values.length - 1)) * (width - 12) + 6;
    const y = height - ((value - min) / spread) * (height - 18) - 10;
    if (index === 0) {
      ctx.moveTo(x, y);
    } else {
      ctx.lineTo(x, y);
    }
  });

  ctx.strokeStyle = "#44e0c4";
  ctx.lineWidth = 2;
  ctx.stroke();
}

export function renderStrategyViz(root, payload) {
  if (!root) {
    return;
  }

  if (!payload) {
    root.innerHTML = '<p class="strategy-route-line">Strategy view is waiting for the next successful price tick.</p>';
    return;
  }

  const { symbol, strategy, opportunity } = payload;
  const opp = opportunity || {};
  const buy = Number(opp.buyPrice || 0);
  const sell = Number(opp.sellPrice || 0);
  const spreadPct = buy > 0 ? ((sell - buy) / buy) * 100 : 0;
  const net = Number(opp.netPerUnit || 0);
  const profitable = !!opp.isProfitable;
  const route = opp.route || "";
  const exBuy = opp.exchangeBuy || "—";
  const exSell = opp.exchangeSell || "—";
  const stratTitle = humanizeStrategyName(strategy);

  const gaugePct = Math.min(100, Math.max(0, spreadPct * 42));
  const gaugeColor = profitable ? "rgba(102, 240, 167, 0.92)" : "rgba(93, 193, 255, 0.65)";
  const stateClass = profitable ? "ok" : "warn";
  const stateText = profitable ? "Above threshold" : "Below threshold";

  root.innerHTML = `
    <div class="strategy-viz-head">
      <span class="strategy-viz-title">${escapeHtml(stratTitle)}</span>
      <span class="strategy-viz-meta">${escapeHtml(symbol || "")} · engine evaluation / tick</span>
    </div>
    <div class="strategy-legs">
      <div class="strategy-leg-card leg-buy">
        <p class="strategy-leg-kicker">Buy leg (min price)</p>
        <p class="strategy-leg-exchange">${escapeHtml(exBuy)}</p>
        <p class="strategy-leg-price">$${formatMoney(buy, 2)}</p>
      </div>
      <div class="strategy-leg-arrow" aria-hidden="true">→</div>
      <div class="strategy-leg-card leg-sell">
        <p class="strategy-leg-kicker">Sell leg (max price)</p>
        <p class="strategy-leg-exchange">${escapeHtml(exSell)}</p>
        <p class="strategy-leg-price">$${formatMoney(sell, 2)}</p>
      </div>
    </div>
    <div class="strategy-viz-footer">
      <div class="strategy-gauge" style="--gauge:${gaugePct.toFixed(1)}; --gauge-color:${gaugeColor}">
        <div class="strategy-gauge-inner">
          <p class="strategy-gauge-label">Spread</p>
          <p class="strategy-gauge-value">${formatMoney(spreadPct, 3)}%</p>
          <p class="strategy-gauge-state ${stateClass}">${stateText}</p>
        </div>
      </div>
      <div>
        <p class="strategy-gauge-label" style="margin-bottom:0.35rem">Net / unit (after fee model)</p>
        <p class="strategy-leg-price" style="margin:0">$${formatMoney(net, 4)}</p>
        <p class="strategy-route-line" style="margin-top:0.65rem">${route ? escapeHtml(route) : "Route details follow the active strategy implementation."}</p>
      </div>
    </div>
  `;
}

function collectExchangesFromHistory(history) {
  const set = new Set();
  history.forEach((sample) => {
    Object.keys(sample.byEx || {}).forEach((k) => set.add(k));
  });
  return [...set].sort((a, b) => a.localeCompare(b));
}

function updateChartLegend(legendEl, exchanges) {
  if (!legendEl) {
    return;
  }
  legendEl.innerHTML = exchanges
    .map((ex, i) => {
      const color = CHART_PALETTE[i % CHART_PALETTE.length];
      return `<span style="--legend-color:${color}"><i></i>${escapeHtml(ex)}</span>`;
    })
    .join("");
}

export function drawLiveSpreadChart(canvas, history, legendEl) {
  const ctx = canvas.getContext("2d");
  if (!ctx) {
    return;
  }

  const width = canvas.clientWidth || 640;
  const height = canvas.clientHeight || 220;
  const dpr = Math.min(window.devicePixelRatio || 1, 2);
  canvas.width = Math.floor(width * dpr);
  canvas.height = Math.floor(height * dpr);
  ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

  const padL = 54;
  const padR = 14;
  const padT = 14;
  const padB = 30;
  const innerW = width - padL - padR;
  const innerH = height - padT - padB;

  ctx.fillStyle = "rgba(7, 14, 24, 0.96)";
  ctx.fillRect(0, 0, width, height);

  if (!history.length) {
    ctx.fillStyle = "rgba(153, 175, 198, 0.75)";
    ctx.font = "13px Space Grotesk, Segoe UI, sans-serif";
    ctx.textAlign = "center";
    ctx.fillText("Collecting live samples…", width / 2, height / 2);
    ctx.textAlign = "left";
    if (legendEl) legendEl.innerHTML = "";
    return;
  }

  const exchanges = collectExchangesFromHistory(history);
  updateChartLegend(legendEl, exchanges);

  if (!exchanges.length) {
    return;
  }

  const carry = {};
  const rows = history.map((sample) => {
    const row = {};
    exchanges.forEach((ex) => {
      if (sample.byEx[ex] != null) {
        carry[ex] = sample.byEx[ex];
      }
      row[ex] = carry[ex];
    });
    return row;
  });

  let yMin = Infinity;
  let yMax = -Infinity;
  rows.forEach((row) => {
    exchanges.forEach((ex) => {
      const v = row[ex];
      if (v != null && !Number.isNaN(v)) {
        yMin = Math.min(yMin, v);
        yMax = Math.max(yMax, v);
      }
    });
  });

  if (!Number.isFinite(yMin) || !Number.isFinite(yMax)) {
    return;
  }

  const padY = Math.max((yMax - yMin) * 0.12, 1);
  yMin -= padY;
  yMax += padY;
  const ySpan = Math.max(yMax - yMin, 1e-9);

  ctx.strokeStyle = "rgba(83, 112, 143, 0.22)";
  ctx.lineWidth = 1;
  for (let g = 0; g <= 4; g += 1) {
    const gy = padT + (innerH * g) / 4;
    ctx.beginPath();
    ctx.moveTo(padL, gy);
    ctx.lineTo(padL + innerW, gy);
    ctx.stroke();
  }

  ctx.fillStyle = "rgba(153, 175, 198, 0.55)";
  ctx.font = "10px JetBrains Mono, Consolas, monospace";
  ctx.textAlign = "right";
  for (let g = 0; g <= 4; g += 1) {
    const gy = padT + (innerH * g) / 4;
    const val = yMax - (ySpan * g) / 4;
    ctx.fillText(formatMoney(val, 0), padL - 6, gy + 3);
  }
  ctx.textAlign = "left";

  const n = rows.length;
  exchanges.forEach((ex, ei) => {
    const color = CHART_PALETTE[ei % CHART_PALETTE.length];
    ctx.strokeStyle = color;
    ctx.lineWidth = 2;
    ctx.beginPath();
    let started = false;
    rows.forEach((row, i) => {
      const v = row[ex];
      if (v == null || Number.isNaN(v)) {
        return;
      }
      const x = n === 1 ? padL + innerW / 2 : padL + (i / (n - 1)) * innerW;
      const y = padT + innerH - ((v - yMin) / ySpan) * innerH;
      if (!started) {
        ctx.moveTo(x, y);
        started = true;
      } else {
        ctx.lineTo(x, y);
      }
    });
    ctx.stroke();

    ctx.fillStyle = color;
    rows.forEach((row, i) => {
      const v = row[ex];
      if (v == null || Number.isNaN(v)) {
        return;
      }
      const x = n === 1 ? padL + innerW / 2 : padL + (i / (n - 1)) * innerW;
      const y = padT + innerH - ((v - yMin) / ySpan) * innerH;
      ctx.beginPath();
      ctx.arc(x, y, n === 1 ? 4 : 3, 0, Math.PI * 2);
      ctx.fill();
    });
  });

  ctx.fillStyle = "rgba(153, 175, 198, 0.5)";
  ctx.font = "10px Space Grotesk, Segoe UI, sans-serif";
  ctx.fillText("time →", padL + innerW - 36, height - 10);
}
