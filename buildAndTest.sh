#!/bin/bash

repositories=("zipkin4net")

for i in $repositories; do
    if [ -d $i ]; then
	pushd $i
	./buildAndTest.sh
	popd
    else
	echo "Can't build $i"
	exit 4
    fi
done
