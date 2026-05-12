const SIMULATED_PROXY_DELAY_MS = 450;

function wait(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

class ApiProxyClient {
  constructor(baseUrl = "") {
    this.baseUrl = baseUrl;
    this.activeControllers = new Map();
    this.authToken = "demo-auth-token";
  }

  async request(requestKey, path, options = {}) {
    const previousController = this.activeControllers.get(requestKey);
    if (previousController) {
      previousController.abort();
    }

    const controller = new AbortController();
    this.activeControllers.set(requestKey, controller);

    try {
      const config = {
        method: options.method || "GET",
        headers: {
          "Content-Type": "application/json",
          "X-Auth-Simulation": this.authToken,
          ...(options.headers || {})
        },
        signal: controller.signal
      };

      if (options.body !== undefined) {
        config.body = JSON.stringify(options.body);
      }

      const [response] = await Promise.all([
        fetch(`${this.baseUrl}${path}`, config),
        wait(SIMULATED_PROXY_DELAY_MS)
      ]);

      if (!response.ok) {
        let errorMessage = `Request failed (${response.status})`;
        try {
          const payload = await response.json();
          errorMessage = payload?.message || payload?.error || errorMessage;
        } catch {
          // Ignore JSON parsing errors on non-JSON responses.
        }
        throw new Error(errorMessage);
      }

      return await response.json();
    } catch (error) {
      if (error.name === "AbortError") {
        throw new Error("Request cancelled by proxy control.");
      }
      throw error;
    } finally {
      if (this.activeControllers.get(requestKey) === controller) {
        this.activeControllers.delete(requestKey);
      }
    }
  }
}

export const apiClient = new ApiProxyClient();
