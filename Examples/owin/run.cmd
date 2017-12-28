@echo off

msbuild owin-example.sln

start backend\bin\Debug\backend.exe "http://localhost:9000"
start frontend\bin\Debug\frontend.exe "http://localhost:8081"