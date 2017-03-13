#!/usr/bin/env bash

declare -a repositories=("zipkin4net" "zipkin4net-aspnetcore")

for i in "${repositories[@]}"; do
    if [ -d $i ]; then
	pushd $i
	./buildAndTest.sh
	popd
    else
	echo "Can't build $i"
	exit 4
    fi
done
