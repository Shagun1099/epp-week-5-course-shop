# Course Shop

Course Shop sells short online courses from several providers. Learners browse the catalogue, add seats to a cart, check out, and see their past purchases.

It's built with ASP.NET Core (Razor Pages and MVC) on .NET 10, using an in-memory database that is re-seeded every time the app starts.

## Run it

You need Docker. From this folder:

```
docker compose up --build
```

Then open http://localhost:5106. If that port is taken, pick another:

```
COURSESHOP_PORT=18106 docker compose up --build
```

## Accounts

Every account's password is `Pass@word1`.

| Account | Role |
|---|---|
| `learner@courseshop.test` | Learner, with a few past purchases |
| `admin@courseshop.test` | Administrator, can edit courses |

## Run the tests

```
docker compose --profile test run --rm tests
```

## Layout

| Folder | What's in it |
|---|---|
| `src/ApplicationCore` | Entities, domain services and query specifications |
| `src/Infrastructure` | Data access and seed data |
| `src/Web` | Pages, controllers, views and view-model services |
| `tests` | Unit, integration and functional tests |

Course Shop is based on [eShopOnWeb](https://github.com/NimblePros/eShopOnWeb), MIT licensed. See `LICENSE`.
