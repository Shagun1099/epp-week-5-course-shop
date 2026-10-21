#!/usr/bin/env bash
# Deploy Course Shop as a public test site on a Linux server.
# Tested target: Ubuntu on AWS. See deployment.md.
#
#   ./scripts/deploy.sh                  first deploy, or redeploy after git pull
#   ./scripts/deploy.sh --domain NAME    use your own domain (first deploy only)
#   ./scripts/deploy.sh --reset          restart everything with fresh seed data
#
# Settings live in ~/courseshop.env, outside the repository.
# Set COURSESHOP_ENV_FILE to use a different path.

set -euo pipefail

cd "$(dirname "$0")/.."

ENV_FILE="${COURSESHOP_ENV_FILE:-$HOME/courseshop.env}"
DOMAIN=""
RESET=false

usage() {
  awk 'NR > 1 && /^#/ { sub(/^# ?/, ""); print; next } NR > 1 { exit }' "$0"
}

while [ $# -gt 0 ]; do
  case "$1" in
    --domain) DOMAIN="${2:?--domain needs a value}"; shift 2 ;;
    --reset) RESET=true; shift ;;
    -h | --help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage >&2; exit 1 ;;
  esac
done

say() { printf '\n==> %s\n' "$1"; }

# 1. Docker
if ! command -v docker >/dev/null 2>&1; then
  if ! command -v apt-get >/dev/null 2>&1; then
    echo "Docker isn't installed, and this script can only install it on Ubuntu or Debian." >&2
    exit 1
  fi
  say "Installing Docker"
  curl -fsSL https://get.docker.com | sudo sh
  sudo usermod -aG docker "$USER"
  echo "Added $USER to the docker group. It takes effect from your next login."
fi

DOCKER=(docker)
if ! docker info >/dev/null 2>&1; then
  DOCKER=(sudo docker)
fi

# 2. Settings file, created once
if [ ! -f "$ENV_FILE" ]; then
  say "Creating $ENV_FILE"

  if [ -z "$DOMAIN" ]; then
    public_ip="$(curl -fsS --max-time 10 https://checkip.amazonaws.com || curl -fsS --max-time 10 https://ifconfig.me)"
    public_ip="$(echo "$public_ip" | tr -d '[:space:]')"
    DOMAIN="${public_ip//./-}.sslip.io"
    echo "No domain given, so using $DOMAIN (sslip.io points it at this server's IP)."
  fi

  umask 077
  cat >"$ENV_FILE" <<SETTINGS
DOMAIN=$DOMAIN
COURSESHOP_ENVIRONMENT=Production
COURSESHOP_SIGNING_KEY=$(openssl rand -base64 48 | tr -d '\n')
COURSESHOP_BIND=127.0.0.1
SETTINGS
elif [ -n "$DOMAIN" ]; then
  echo "Note: $ENV_FILE already exists, so --domain is ignored. Edit the file to change it." >&2
fi

setting() { grep -E "^$1=" "$ENV_FILE" | head -n1 | cut -d= -f2- | tr -d '"'; }
SITE_DOMAIN="$(setting DOMAIN)"

COMPOSE=("${DOCKER[@]}" compose --env-file "$ENV_FILE" --profile hosted)

# 3. Start
if [ "$RESET" = true ]; then
  say "Stopping everything, so the seed data is restored on start"
  "${COMPOSE[@]}" down
fi

say "Building and starting (the first time takes several minutes)"
"${COMPOSE[@]}" up -d --build

# 4. Check it answers over HTTPS with the seeded courses.
say "Waiting for https://$SITE_DOMAIN"
curl_opts=(-fsS --max-time 5)
if [ "$SITE_DOMAIN" = "localhost" ]; then
  curl_opts+=(-k)
fi
for _ in $(seq 1 90); do
  if curl "${curl_opts[@]}" "https://$SITE_DOMAIN/" 2>/dev/null | grep -q "Data Storytelling Basics"; then
    cat <<DONE

Course Shop is up.

  Site:           https://$SITE_DOMAIN
  Demo learner:   learner@courseshop.test / Pass@word1

Anyone with the address can register an account. All data is made up.
DONE
    exit 0
  fi
  sleep 5
done

cat >&2 <<HELP

The site didn't answer within 7.5 minutes. Check that:
  - ports 80 and 443 are open to the internet
  - $SITE_DOMAIN points at this server
  - the server has at least 2 GB of memory
Then read the logs:
  ${COMPOSE[*]} logs proxy web
HELP
exit 1
