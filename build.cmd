@echo off
cls

nuget restore packages.config -PackagesDirectory build-packages
build-packages\FAKE.4.63.0\tools\FAKE.exe build.fsx %*