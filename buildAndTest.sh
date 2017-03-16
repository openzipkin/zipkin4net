#!/usr/bin/env bash

declare -a repositories=("zipkin4net" "zipkin4net-aspnetcore")

for i in "${repositories[@]}"; do
    if [ -d $i ]; then
	pushd $i
	./buildAndTest.sh
        if [ $? -ne 0 ]; then
          echo "Compilation of $i failed, exiting"
          exit 1
        fi
	popd
    else
	echo "Can't build $i"
	exit 4
    fi
done
