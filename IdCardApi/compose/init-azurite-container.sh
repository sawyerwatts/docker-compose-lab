#!/bin/bash

set -euo pipefail
IFS=$'\n\t'
# Note that `set +e` is the syntax to disables variable strictness. This is
# particularly helpful if you need to source a script that violates any of these
# `set`s.

container=${1:-}
if [[ -z "$container" ]]
then
  echo "usage: $(basename "$0") CONTAINER_NAME"
  exit 1
fi

if [[ -z "$AZURE_STORAGE_CONNECTION_STRING" ]]
then
  echo "Ensure environment variable AZURE_STORAGE_CONNECTION_STRING is configured"
  exit 1
fi

num_containers=$(az storage container list | jq "map(select(.name==\"$container\"))" | jq 'length')
if [ "$num_containers" -eq "0" ]
then
  echo -e "\n> Confirmed that the container $container does not exist, will begin initialization"
elif [ "$num_containers" -eq "1" ]
then
  echo -e "\n> The container $container already exists, will not initialize"
  exit 0
else
  echo -e "\n> Somehow more than one container was found with name $container ($num_containers) - this is a coding bug"
  exit 1
fi

echo -e "\n> Creating container $container"
az storage container create --name "$container"

container_dir="./azurite/containers/$container"
if [[ ! -d "$container_dir" ]]
then
  echo -e "\n> Could not find a container seed directory at $container_dir, skipping"
  exit 0
fi

find "$container_dir" -type f -print0 | while read -r -d '' file
do
  blob=${file:${#container_dir}+1}
  echo -e "\n> Uploading file $file as blob $blob"
  az storage blob upload \
    --file "$file" \
    --name "$blob" \
    --container "$container"
done

echo -e "\n> Completed normally"
