# openPDC-adapter
openPDC-adapter for retrieving items from a "Products and Services catalog" (Producten en Diensten Catalogus) and syncing them into the Open Object register.

This adapter is a generic standalone application, developed for the municipality Rheden to make their Products and Services catalog directly available in KISS (https://www.kiss-klantcontact.nl/).
The openPDC-adapter fetches products from the WordPress REST API and syncs them as SDG Kennisartikelen into Open Objects.

<img width="800px" alt="KISS Rheden Context Diagram" src="https://github.com/user-attachments/assets/3e18edb7-d584-4a8e-88b1-9a87255e104a" />

---

## How it works

1. **Read** — streams all products from the WordPress REST API (`/wp/v2/product`) with Basic Auth, handling pagination automatically via `X-WP-TotalPages` response headers
2. **Map** — converts each item to an SDG Kennisartikel object matching the [kennisartikel schema](https://github.com/open-objecten/objecttypes/blob/main/community-concepts/PDC%20-%20kennisartikel/kennisartikel-schema.json)
3. **Insert, update, delete** — DELETEs Kennisartikelen that no longer exist in the source, UPDATEs those already in the Open Object register, and INSERTs new ones

## Prerequisites

| Requirement | Version |
|---|---|
| WordPress instance with products | accessible over HTTP, application password configured |
| [Open Objects API](https://github.com/maykinmedia/objects-api) | running and configured with the 'Kennisartikel' object type |

#### Running Open Objects with Docker

To run Open Objects via `docker-compose`, create a `docker/postgres.entrypoint-initdb.d/` directory **in the same directory as your `docker-compose.yml`** and populate it with the DB initialisation scripts from:

> https://github.com/maykinmedia/open-object/tree/master/docker/postgres.entrypoint-initdb.d

## Configuration reference

All values can be set as environment variables or in `appsettings.json`. Environment variables take precedence.

| Key | Description | Required |
|---|---|---|
| `OpenPdc__BaseUrl` | Base URL of the WordPress REST API — e.g. `https://example.nl/wp-json/wp/v2/` | **Yes** |
| `OpenPdc__Username` | WordPress username for Basic Auth | **Yes** |
| `OpenPdc__Password` | WordPress application password for Basic Auth | **Yes** |
| `OpenPdc__ItemBaseUrl` | Base URL used to build per-item URLs in the mapped object — e.g. `https://example.nl/wp-json/wp/v2/product` | **Yes** |
| `OpenObjects__Token` | API token for `Authorization: Token <value>` | **Yes** |
| `OpenObjects__ObjectTypeUrl` | URL of the registered object type — e.g. `http://host/api/v2/objecttypes/<uuid>` | **Yes** |
| `OpenObjects__ObjectTypeVersion` | Version number of the object type — e.g. `1` | **Yes** |
| `OpenObjects__OwmsUrl` | URL of the responsible organisation | **Yes** |
| `OpenObjects__OwmsIdentifier` | OWMS identifier URI of the organisation | **Yes** |
| `OpenObjects__OwmsEndDate` | OWMS end date (ISO 8601) | No (defaults to `2099-12-31`) |

## Constant field values

The following fields in the mapped OpenObjects Kennisartikelen type are hardcoded:

| Field | Value | Notes |
|---|---|---|
| `uuid` | `00000000-0000-0000-0000-{itemId:D12}` | Deterministic per-item UUID derived from the WordPress product ID — Elasticsearch deduplicates by UUID, so each item needs a unique one |
| `upnUri` | `"unknown"` | Not available in the source data |
| `productAanwezig` | `true` | Defaults to true |
| `doelgroep` | `"eu-burger"` | Fixed target audience |
| `taal` | `"nl"` | |

## Running

```bash
dotnet run --project src/OpenPdc.Worker/OpenPdc.Worker.csproj
```
