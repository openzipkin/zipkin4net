@echo off

msbuild zipkin4net-owin-example.sln

start backend\backend\bin\Debug\backend.exe "http://localhost:9000"
start frontend\frontend\bin\Debug\frontend.exe "http://localhost:8081"