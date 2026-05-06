# openPDC-adapter
openPDC-adapter for retrieving items from a "Products and Services catalog" (Producten en Diensten Catalogus) and syncing them into the Open Object register.

This adapter is a generic standalone application, developed for the municiplaity Rheden to make their Products and Services catalog directly avalibale in KISS (https://www.kiss-klantcontact.nl/).
The openPDC-adapter is developed and tested with a openPDC WorkdPress plugin
<img width="800px"   alt="KISS Rheden Context Diagram" src="https://github.com/user-attachments/assets/3e18edb7-d584-4a8e-88b1-9a87255e104a" />
 
---

## How it works

1. **Read** — streams all PDC items from the openPDC WordPress REST API (handles pagination automatically)
2. **Map** — converts each item to an SDG Kennisartikel object matching the [kennisartikel schema](src/docs/kennisartikel-schema.json)
3. **Write** — POSTs each mapped object to the Open Objects API

## Prerequisites

| Requirement | Version |
|---|---|
| openPDC WordPress instance | accessible over HTTP |
| [Open Objects API](https://github.com/maykinmedia/objects-api) | running and configured with an object type (see src/docs/kennisartikel-schema.json) |

### Configuration reference

All values can also be set as real environment variables or in `appsettings.json`. Environment variables take precedence.

| Key | Description | Required |
|---|---|---|
| `OpenObjects__BaseUrl` | Base URL of the Open Objects API | No (defaults to `http://localhost:8000`) |
| `OpenObjects__Token` | API token for `Authorization: Token <value>` | **Yes** |
| `Migration__ObjectTypeUrl` | Full URL of the registered object type in Open Objects | **Yes** |
| `Migration__PdcItemBaseUrl` | Base URL for building per-item URLs, without trailing slash | **Yes** |
| `Migration__OwmsUrl` | URL of the responsible organisation | **Yes** |
| `Migration__OwmsIdentifier` | OWMS identifier URI of the organisation | **Yes** |
| `Migration__OwmsEndDate` | OWMS end date (ISO 8601) | No (defaults to `2099-12-31`) |
| `Migration__Doelgroep` | Target audience: `eu-burger` or `eu-bedrijf` | No (defaults to `eu-bedrijf`) |

## Running

```bash
dotnet run --project src/OpenPdc.Sample/OpenPdc.Sample.csproj
```
