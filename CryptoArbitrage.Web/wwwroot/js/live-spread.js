/**
 * Live spread polling client for Prices page.
 * Polls /api/market/spread every 1000ms and updates live spread display.
 */

(function () {
    // Ensure symbol select has options
    const symbolSelect = document.getElementById('liveSymbol');
    if (symbolSelect && symbolSelect.children.length === 0) {
        ['BTC', 'ETH'].forEach(sym => {
            const opt = document.createElement('option');
            opt.value = sym;
            opt.textContent = sym;
            symbolSelect.appendChild(opt);
        });
    }

    // Get DOM elements
    const spreadStatus = document.getElementById('spreadStatus');
    const binanceBid = document.getElementById('binanceBid');
    const binanceAsk = document.getElementById('binanceAsk');
    const krakenBid = document.getElementById('krakenBid');
    const krakenAsk = document.getElementById('krakenAsk');
    const spreadValue = document.getElementById('spreadValue');
    const spreadPct = document.getElementById('spreadPct');
    const chartLastSync = document.getElementById('chartLastSync');

    // Skip if elements missing
    if (!symbolSelect || !spreadStatus) {
        console.warn('Live spread elements not found on page');
        return;
    }

    /**
     * Format decimal to currency (e.g., 65123.45)
     */
    function formatCurrency(value) {
        if (value === null || value === undefined || value === '-') return '-';
        const num = parseFloat(value);
        return num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 8 });
    }

    /**
     * Format decimal to percentage (e.g., 0.25%)
     */
    function formatPercent(value) {
        if (value === null || value === undefined || value === '-') return '-';
        const num = parseFloat(value);
        return num.toFixed(4);
    }

    /**
     * Format time as HH:mm:ss
     */
    function formatTime(date) {
        const h = String(date.getHours()).padStart(2, '0');
        const m = String(date.getMinutes()).padStart(2, '0');
        const s = String(date.getSeconds()).padStart(2, '0');
        return `${h}:${m}:${s}`;
    }

    /**
     * Poll the spread endpoint
     */
    async function pollSpread() {
        const symbol = (symbolSelect.value || 'BTC').toUpperInvariant();

        // Check for unsupported symbols
        if (!['BTC', 'ETH'].includes(symbol)) {
            spreadStatus.textContent = 'Unsupported symbol (only BTC/ETH live for now).';
            return;
        }

        try {
            const response = await fetch(`/api/market/spread?symbol=${symbol}`);

            if (response.status === 404) {
                spreadStatus.textContent = 'Loading live quotes...';
                return;
            }

            if (!response.ok) {
                spreadStatus.textContent = 'Error loading spread';
                return;
            }

            const data = await response.json();

            // Update exchange bids/asks
            const binanceEx = data.exchanges.find(e => e.name === 'binance');
            const krakenEx = data.exchanges.find(e => e.name === 'kraken');

            if (binanceEx) {
                binanceBid.textContent = formatCurrency(binanceEx.bid);
                binanceAsk.textContent = formatCurrency(binanceEx.ask);
            }

            if (krakenEx) {
                krakenBid.textContent = formatCurrency(krakenEx.bid);
                krakenAsk.textContent = formatCurrency(krakenEx.ask);
            }

            // Update spread
            if (data.best) {
                spreadValue.textContent = formatCurrency(data.best.grossSpread);
                spreadPct.textContent = formatPercent(data.best.grossSpreadPct);
            }

            // Update status and sync time
            const ts = new Date(data.timestampUtc);
            const now = new Date();
            const ageMs = now - ts;
            const ageSec = Math.floor(ageMs / 1000);
            spreadStatus.textContent = `Updated ${ageSec}s ago`;

            if (chartLastSync) {
                chartLastSync.textContent = `Updated ${formatTime(now)}`;
            }

        } catch (error) {
            console.warn('Spread poll error:', error);
            spreadStatus.textContent = 'Disconnected';
        }
    }

    // Initial poll and then every 1000ms
    pollSpread();
    setInterval(pollSpread, 1000);
})();
