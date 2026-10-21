# Deploying the test site

`scripts/deploy.sh` runs Course Shop as a public test site on a Linux server, for QA engineers and designers. Tested target: Ubuntu on AWS. It works on any Ubuntu or Debian server with a public IP.

It uses the same code as local development, with these differences:

- Caddy serves the site over HTTPS, and tells search engines not to index it. `robots.txt` blocks them too.
- Only Caddy is reachable from outside. The app itself listens on the server only.
- The app runs in production mode, so errors show a plain error page, not a stack trace.
- The demo accounts are created as usual. Anyone with the address can register an account. All data is made up.

## The server

- Ubuntu, with at least 2 GB of memory (on AWS, a `t3.small`). The first build needs most of it.
- Ports 80 and 443 open to everyone, and 22 open for SSH. Don't open 5106.
- A domain is optional. Without one, the site gets an address like `3-98-12-45.sslip.io`. It points at the server's IP and gets a real HTTPS certificate with no DNS setup.

## First deployment

1. Connect with agent forwarding, so the server can clone the repository using your own GitHub key, without storing anything on the server:

   ```bash
   ssh -A ubuntu@<server address>
   ```

2. Clone the repository and run the script:

   ```bash
   git clone git@github.com:Prepr/epp-week-5-course-shop.git
   cd epp-week-5-course-shop
   ./scripts/deploy.sh
   ```

   To use your own domain, point its DNS at the server first, then run `./scripts/deploy.sh --domain test.example.com`.

The script installs Docker if it's missing, creates `~/courseshop.env` with the site's settings, builds and starts everything, waits until the course list loads over HTTPS, then prints the site address.

## Everyday use

Run these from the repository folder on the server.

| To | Run |
|---|---|
| Redeploy after pulling new code | `git pull && ./scripts/deploy.sh` |
| Restore the seed data | `./scripts/deploy.sh --reset` |
| Restart after a server reboot | `./scripts/deploy.sh` |
| Read the logs | `docker compose --env-file ~/courseshop.env --profile hosted logs -f web` (straight after the first deployment, start it with `sudo`, or log in again) |

The app keeps its data in memory, so any restart restores the seed data. Use `--reset` if testers fill the site with junk.
