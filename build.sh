nuget restore ./packages.config -PackagesDirectory ./build-packages \
&& mono build-packages/FAKE.4.63.0/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx