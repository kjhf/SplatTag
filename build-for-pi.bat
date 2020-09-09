cd ./SplatTag
dotnet publish -r linux-arm -o "../release"
cd ../
tar.exe -cvzf _pi.tar "./release"
pause