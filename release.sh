#!/bin/sh

if [ $# -lt 1 ]; then
  echo "Usage ./release.sh version"
  exit 1
fi

version=$1

git checkout master
git pull origin
git tag $version
git push origin --tags
