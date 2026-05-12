# 🚀 CryptoArbitrage - Trading Platform

O aplicație .NET 9.0 complexă care implementează strategii de arbitraj crypto și demonstrează principiile SOLID și pattern-urile de design moderne.

## 📋 Descriere Proiect

**CryptoArbitrage** este o platformă de trading care:
- ✅ Monitorizează prețurile criptomonedelor pe mai multe exchange-uri (Binance, Kraken)
- ✅ Identifică oportunități de arbitraj între platforme
- ✅ Execută tranzacții automate în funcție de strategii configurabile
- ✅ Demonstrează implementarea pattern-urilor de design și principiilor SOLID
- ✅ Oferă o interfață web pentru gestionarea și monitorizarea botului

---

## 🏗️ Arhitectura Proiectului

### Structura Soluției

```
CryptoArbitrage/
├── CryptoArbitrage.Engine/          # Console app - Demonstrarea pattern-urilor
│   ├── Program.cs                   # Entry point cu exemple de design patterns
│   ├── DesignPatterns/
│   │   ├── AbstractFactory/         # Pattern: Abstract Factory
│   │   ├── FactoryMethod/           # Pattern: Factory Method
│   │   ├── Behavioral/              # Behavioral patterns (Observer, Command, Memento)
│   │   └── Structural/              # Structural patterns (Facade, Adapter, Composite)
│   └── Solid/                       # Exemplificarea principiilor SOLID
│
└── CryptoArbitrage.Web/             # ASP.NET Core Web Application
    ├── Program.cs                   # Configurare servicii și middleware
    ├── Controllers/                 # API endpoints și page handlers
    │   ├── DashboardController      # Dashboard-ul principal
    │   ├── TradeApiController       # API pentru tranzacții
    │   ├── WalletController         # Gestiune portofel
    │   ├── PricesController         # Prețuri criptomonede
    │   └── CryptoApiController      # API crypto
    ├── Services/
    │   ├── Auth/                    # Autentificare utilizatori
    │   └── Persistence/             # Persistență date (SQL Server)
    ├── Models/                      # Modele de date
    ├── ViewModels/                  # ViewModel-uri pentru views
    └── Views/                       # Razor views și UI
```

---

## 🔧 Tehnologiile Utilizate

| Tehnologie | Versiune | Utilizare |
|-----------|---------|-----------|
| **.NET** | 9.0 | Runtime framework |
| **ASP.NET Core** | Latest | Web framework |
| **SQL Server** | - | Bază de date persistență |
| **C#** | Latest | Limbaj de programare |
| **MVC** | - | Arhitectura web |

---

## 📚 Design Patterns Implementate

### 1. **Factory Method Pattern** 
Locație: `CryptoArbitrage.Engine/DesignPatterns/FactoryMethod/`
- `ProfitReportCreator` - Creează rapoarte de profit
- `RiskReportCreator` - Creează rapoarte de risc

### 2. **Abstract Factory Pattern**
Locație: `CryptoArbitrage.Engine/DesignPatterns/AbstractFactory/`
- `BinanceTradingFactory` - Factory pentru Binance
- `KrakenTradingFactory` - Factory pentru Kraken
- `ITradingInstrumentsFactory` - Interfață comună

### 3. **Composite Pattern**
Locație: `CryptoArbitrage.Engine/DesignPatterns/Structural/Composite/`
- Structuri ierarhice de componente

### 4. **Facade Pattern**
Locație: `CryptoArbitrage.Engine/DesignPatterns/Structural/Facade/`
- Simplificarea interfețelor complexe

### 5. **Adapter Pattern**
Locație: `CryptoArbitrage.Engine/DesignPatterns/Structural/Adapter/`
- Compatibilitate între interfețe diferite

### 6. **Observer Pattern**
Utilizare: Monitorizare schimbări de preț și stat bot
- `Observer` pattern din behavioral patterns

### 7. **Command Pattern**
Utilizare: Execuția comenzilor de trading
- `TradingBotInvoker` - Invoker pentru comenzi

### 8. **Memento Pattern**
Utilizare: Salvarea și restaurarea stării botului
- `ArbitrageBotStateOriginator` - Originator
- `StateManager` - Caretaker

---

## 🎯 Principiile SOLID Implementate

Locație: `CryptoArbitrage.Web/Solid/`

### S - Single Responsibility Principle
- Fiecare service are o singură responsabilitate
- Controllers sunt responsabili doar de HTTP handling
- Services conțin logica de business

### O - Open/Closed Principle
- `IExchangeService` - Interfață deschisă pentru extensie
- `IArbitrageStrategy` - Strategii noi fără modificare existente

### L - Liskov Substitution Principle
- Implementări ale `IExchangeService` (BinanceService, KrakenService)
- Implementări ale `IArbitrageStrategy`

### I - Interface Segregation Principle
- Interfețe specifice: `IPriceProvider`, `IFeeCalculator`, `IArbitrageStrategy`
- Interfețe de persistență: `IBotStateStore`, `IUserStore`

### D - Dependency Inversion Principle
- Injecție de dependențe prin constructor
- Servicii registrate în container-ul IoC (Startup)

---

## 🌟 Caracteristici Principale

### 🤖 Engine de Arbitraj
- Monitorizare în timp real a prețurilor
- Calculare oportunități de arbitraj
- Strategie configurabilă de trading
- Gestionare comisioane exchange

### 🖥️ Interfață Web
- Dashboard pentru vizualizare status bot
- Gestionare portofel (Wallet)
- Istoric tranzacții
- Autentificare utilizatori
- API REST pentru operații externe

### 💾 Persistență
- Salvare stare bot (Memento pattern)
- Stocare date utilizatori
- SQL Server integration

### 🔐 Securitate
- Autentificare utilizatori
- Session management
- Validare input

---

## 🚀 Instalare și Rulare

### Prerequisite
- .NET 9.0 SDK
- SQL Server (local sau remote)
- Visual Studio 2022+ / Rider / VS Code

### Pași de instalare

1. **Clone repository**
   ```bash
   git clone <repository-url>
   cd CryptoArbitrage
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Configurare SQL Server**
   - Actualizați connection string în `appsettings.json`
   - Rulați migrations (dacă sunt configurate)

4. **Build proiectul**
   ```bash
   dotnet build
   ```

### Rulare

#### Engine (Console App - Demo pattern-uri)
```bash
cd CryptoArbitrage.Engine
dotnet run
```

Output: Demonstrație a tuturor design pattern-urilor

#### Web Application (ASP.NET Core)
```bash
cd CryptoArbitrage.Web
dotnet run
```

Accesați: `https://localhost:5001` (sau configurat în launchSettings.json)

---

## 📊 Fluxul Aplicației

```
┌─────────────────────────────────────────────────────────┐
│  USER (Web Browser)                                     │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼
        ┌─────────────────────┐
        │ ASP.NET Core Web    │
        │ (MVC Controllers)   │
        └────────┬────────────┘
                 │
        ┌────────┴──────────────────────────────────┐
        │                                           │
        ▼                                           ▼
    ┌──────────────┐                        ┌──────────────┐
    │ Authentication │                       │ Services Layer│
    │ & Session      │                       │              │
    └──────────────┘                        └────┬─────────┘
                                                 │
        ┌────────────────────────────────────────┴──────────────┐
        │                                                        │
        ▼                                                        ▼
    ┌─────────────────┐                                 ┌────────────────┐
    │ ArbitrageEngine │                                 │ CryptoPriceServ│
    │                 │                                 │                │
    └────────┬────────┘                                 └────┬───────────┘
             │                                               │
      ┌──────┴──────┐                                        │
      │             │                                        │
      ▼             ▼                                        ▼
   ┌─────┐   ┌──────────────┐                         ┌──────────┐
   │Fee  │   │Strategy      │                         │ Exchange │
   │Calc │   │              │                         │ Services │
   └─────┘   └──────────────┘                         └──────────┘
                                                    (Binance, Kraken)
```

---

## 🔌 API Endpoints

### Trading API
- `POST /api/trade/execute` - Executare tranzacție
- `GET /api/trade/history` - Istoric tranzacții
- `POST /api/trade/validate` - Validare tranzacție

### Wallet API
- `GET /api/wallet/balance` - Sold portofel
- `GET /api/wallet/positions` - Poziții deschise
- `POST /api/wallet/transaction` - Transaction

### Prices API
- `GET /api/prices/current` - Prețuri curente
- `GET /api/prices/pair/{pair}` - Preț pereche specifică

### Bot State
- `GET /api/botstate` - Status curent bot
- `POST /api/botstate/save` - Salvare stare

---

## 📝 Configurare

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CryptoArbitrage;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

## 🧪 Testing

Structura pentru teste unitare și integrare:
- Unit tests pentru services
- Integration tests pentru API endpoints
- Pattern verification tests

```bash
dotnet test
```

---

## 📖 Documentație Internă

- `BEHAVIORAL_PATTERNS_EXPLICATIE_COMPLETA.txt` - Documentație detaliată pattern-uri comportamentale
- Code comments în fiecare pattern class
- XML documentation pentru public APIs

---

## 🤝 Contribuții

1. Fork repository
2. Creaţi branch pentru feature: `git checkout -b feature/AmazingFeature`
3. Commit schimbări: `git commit -m 'Add some AmazingFeature'`
4. Push la branch: `git push origin feature/AmazingFeature`
5. Open Pull Request

---

## 📄 Licență

Specify your license here (MIT, Apache 2.0, etc.)

---

## 👨‍💻 Autor

CryptoArbitrage Project Team

---

## 📞 Contact & Support

- 📧 Email: your-email@example.com
- 🐛 Issues: GitHub Issues
- 💬 Discussions: GitHub Discussions

---

## 🗺️ Roadmap

- [ ] Integrare mai multe exchange-uri (Coinbase, Kraken API v2)
- [ ] Machine Learning pentru predictie preț
- [ ] Dashboard avançat cu grafice real-time
- [ ] Mobile app (Flutter/React Native)
- [ ] WebSocket pentru live price updates
- [ ] Advanced strategy builder UI
- [ ] Notificări push
- [ ] API publik pentru integrări externe

---

## ⭐ Key Learning Points

Acest proiect demonstrează:
1. ✅ Implementare corectă a design pattern-urilor
2. ✅ Aderență la principiile SOLID
3. ✅ Arhitectură scalabilă și maintainable
4. ✅ Injecție de dependențe și IoC container
5. ✅ ASP.NET Core best practices
6. ✅ Separare între business logic și UI
7. ✅ Persistență și state management

---

**Last Updated:** 2026-05-12  
**Version:** 1.0.0
