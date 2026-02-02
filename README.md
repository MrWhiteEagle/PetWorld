# PetWorld - System AI dla Sklepu Zoologicznego

Aplikacja demonstracyjna realizująca zadanie rekrutacyjne: sklep internetowy z inteligentnym asystentem AI (Writer-Critic workflow).

## 📋 Spis treści
- [O projekcie](#o-projekcie)
- [Architektura](#architektura)
- [Wymagania i Funkcjonalności](#wymagania-i-funkcjonalności)
- [Uruchomienie](#uruchomienie)
- [Struktura Katalogów](#struktura-katalogów)

## 🐾 O projekcie
PetWorld to system umożliwiający klientom sklepu zoologicznego interakcję z inteligentnym asystentem. System wykorzystuje zaawansowany workflow **Writer-Critic** oparty na modelach językowych (LLM/OpenAI), aby zapewnić dokładne, bezpieczne i pomocne odpowiedzi.

### Scenariusz biznesowy
Klienci pytają o produkty dla swoich zwierząt. Asystent:
1. Analizuje pytanie.
2. Wyszukuje produkty z katalogu.
3. Generuje odpowiedź (Agent Writer).
4. Weryfikuje odpowiedź pod kątem jakości błędów (Agent Critic).
5. Poprawia odpowiedź w pętli (max 3 iteracje), zanim trafi do klienta.

## 🏗 Architektura
Projekt został zrealizowany zgodnie z zasadami **Clean Architecture (Onion Architecture)**:

1.  **Domain (`src/Domain`)**:
    *   Rdzeń systemu. Zawiera klasy biznesowe (`Product`, `ChatEntry`, `ChatMessage`).
    *   Brak zależności do innych warstw.

2.  **Application (`src/Application`)**:
    *   Logika aplikacji. Zawiera interfejsy (`IAiChatService`, `IApplicationDbContext`), DTO (`ChatResultDto`) oraz logikę biznesową niezależną od infrastruktury.

3.  **Infrastructure (`src/Infrastructure`)**:
    *   Implementacja interfejsów warstwy aplikacji.
    *   Dostęp do danych (`ApplicationDbContext` - Entity Framework Core / MySQL).
    *   Implementacja serwisów AI (`AiChatService` z użyciem `Microsoft.Extensions.AI`).
    *   Zawiera logikę seedowania bazy danych (katalog produktów).

4.  **WebUI (`src/WebUI`)**:
    *   Warstwa prezentacji oparta na **Blazor Server**.
    *   Interfejs użytkownika (Chat, Historia).
    *   Konfiguracja kontenera DI i `Program.cs`.

## ✅ Wymagania i Funkcjonalności

Projekt spełnia wszystkie postawione wymagania rekrutacyjne:

*   [x] **Clean Architecture**: Zachowany ścisły podział na warstwy.
*   [x] **Blazor Server**: Interaktywne UI z wykorzystaniem SignalR.
*   [x] **Integracja AI**: Implementacja wzorca Writer-Critic (Microsoft Agent Framework / Semantic Kernel concepts).
*   [x] **Baza danych MySQL**: Przechowywanie produktów i historii rozmów.
*   [x] **Docker Compose**: Jedno polecenie do uruchomienia całego środowiska.

### Funkcje AI (Writer-Critic)
*   **Writer Agent**: Generuje odpowiedzi na podstawie kontekstu produktów z bazy danych.
*   **Critic Agent**: Ocenia odpowiedzi (format JSON) i wymusza poprawki, jeśli odpowiedź nie spełnia kryteriów (np. brak cen, język inny niż polski).
*   **Iteracje**: System wykonuje do 3 prób poprawy odpowiedzi przed wyświetleniem jej użytkownikowi.

## 🚀 Uruchomienie

### Wymagania wstępne
*   **Docker** oraz **Docker Compose** zainstalowane na maszynie.
*   Klucz API OpenAI (opcjonalnie, bez klucza działa w trybie Mock).

### Instrukcja

1.  Sklonuj repozytorium.
2.  Wyeksportuj klucz API OpenAI jako zmienną środowiskową (lub sformatuj plik `.env` / `docker-compose.override.yml`, ale najprościej zmienną w shellu):

    ```bash
    export OPENAI_API_KEY="sk-..."
    ```

    Alternatywnie edytuj `src/WebUI/appsettings.json` lub przekaż w `docker-compose.yml`.
    **Uwaga:** Jeśli nie podasz klucza, aplikacja uruchomi się z **MockAiChatService** (symulacja odpowiedzi).

3.  Uruchom aplikację poleceniem:

    ```bash
    docker compose up
    ```

    *System automatycznie zbuduje obrazy, uruchomi bazę MySQL, utworzy schemat bazy i załaduje przykładowe dane (katalog produktów).*

4.  Otwórz przeglądarkę pod adresem:
    **[http://localhost:5000](http://localhost:5000)**

## 📂 Struktura Katalogów

```text
src/
├── Domain/           # Klasy Obiektów (Product, ChatEntry)
├── Application/      # Interfejsy, DTOs
├── Infrastructure/   # EF Core (MySQL), AiChatService
└── WebUI/            # Blazor Server App (Pages, Components)
docker-compose.yml    # Kontener
```

## 🛠 Technologie
*   .NET 8
*   Blazor Server
*   Entity Framework Core
*   MySQL 8.0
*   Microsoft.Extensions.AI / OpenAI API
*   Docker
