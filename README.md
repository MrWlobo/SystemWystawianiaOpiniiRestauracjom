# System do wystawiania opinii restauracjom

## Autorzy
  - Kacper Dłubała - kacperd@student.agh.edu.pl
  - Michał Dworniczak - mdwornic@student.agh.edu.pl

## Opis

### Baza danych
  - restauracje
    - id restauracji
    - nazwa
    - id rodzaju kuchni
  - adresy
    - miasto
    - ulica
    - numer domu
  - opinie
    - uzytkownik wystawiajacy
    - id restauracji otrzymujacej
    - ilosc gwiazdek
    - komentarz
  - użytkownicy
    - id uzytkownika
    - login
    - haslo
    - uprawnienia
  - rodzaje kuchni
    - id rodzaju kuchni
    - nazwa rodzaju

### Widoki
  - logowanie, rejestracja
  - ranking restauracji
  - filtrowanie restauracji po rodzajach kuchni
  - widok jednej restauracji i jej opinii
  - dodawanie opinii
  - dodawanie restauracji

## Do czego służy aplikacja?
  Aplikacja służy przede wszystkim do wystawiania ocen restauracjom przez użytkowników oraz ich przeglądania. Oprócz tego pozwala dodać do bazy danych informacje o nowej restauracji, a także wyświetlić ich ranking.

## Funkcjonalności
  - Obsługa kont
    - Logowanie
    - Rejestracja
    - Wylogowanie
  - Dostępne przed zalogowaniem
    - Wyświetlenie informacji o wszystkich restauracjach
    - Wyświetlenie rankingu restauracji po średniej ocen
    - Flitrowanie restauracji po kuchni
    - Przeglądanie ocen wybranej restauracji
  - Dostępne po zalogowaniu
    - Wszystkie dostępne przed zalogowaniem
    - Dodanie opinii
    - Dodanie restauracji

