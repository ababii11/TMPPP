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

export function renderPriceCards(container, prices) {
  container.innerHTML = "";

  if (!prices.length) {
    container.innerHTML = '<p class="empty-message">No price data available.</p>';
    return;
  }

  prices.forEach((item) => {
    const card = document.createElement("article");
    card.className = "crypto-price-item";

    card.innerHTML = `
      <h3>${item.symbol}</h3>
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
