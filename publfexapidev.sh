#!/bin/bash
echo ">>进入打包项目路径"
cd /Users/topbrids/Desktop/lfex/lfexApi/src/lfexApi
echo ">>查看当前路径下文件"
ls
echo ">>执行打包发布命令"
dotnet publish -c debug -o /Users/topbrids/Desktop/lfex/lfexApi/release/lfexapi_dev
echo ">>进入到打包好的路径查看"
cd /Users/topbrids/Desktop/lfex/lfexApi/release/
ls
echo ">>打成压缩包"
tar -zcvf lfexapi_dev.tar.gz lfexapi_dev
echo ">>上传到服务器 请输入密码:"
scp lfexapi_dev.tar.gz root@129.28.186.13:/apps/project/lfex
echo ">>密码远端连接服务器 请输入密码:"
ssh root@129.28.186.13

# echo ">>进入到服务器项目发布路径"
# cd /apps/project/lfex

# echo ">>解压压缩包到当前路径"
# tar -zxvf lfexapi_dev.tar.gz

