#!/bin/bash

set -euo pipefail
IFS=$'\n\t'
# Note that `set +e` is the syntax to disables variable strictness. This is
# particularly helpful if you need to source a script that violates any of these
# `set`s.

if [[ -z "$MSSQL_SA_PASSWORD" ]]
then
  echo -e "\n> The environment variable MSSQL_SA_PASSWORD is not configured."
  exit 1
fi

db=${1:-}
if [[ -z "$db" ]]
then
  echo "usage: $(basename "$0") DB_NAME"
  exit 1
fi

sqlcmd_prefix="/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U SA -P '$MSSQL_SA_PASSWORD' -b"

echo -e "\n> Dropping DB $db (if exists)"
bash -c "$sqlcmd_prefix -Q 'drop database if exists $db'"

echo -e "\n> Creating DB $db"
bash -c "$sqlcmd_prefix -Q 'create database $db'"


if [[ ! -d "./$db/" ]]
then
  echo -e "\n> Could not find a directory named $db, not applying any migrations"
  exit 0
fi

echo -e "\n> Finding and applying SQL files"
find "./$db" -type f -name "*.sql" -print0 | while read -r -d '' file
do
  echo -e "\n> Applying $file"
  /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U SA -P "$MSSQL_SA_PASSWORD" -b -d "$db" -i "$file"
done

echo -e "\n> Completed normally"
