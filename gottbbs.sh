echo "go in service"
ssh -i /Users/topbrids/cert/testbbs.pem root@101.32.178.79

# https://lfex-1254396143.cos.ap-hongkong.myqcloud.com/app/lfexapp-v1.1.0-android.apk

# scp -i /Users/topbrids/cert/testbbs.pem /Users/topbrids/Downloads/index.html root@101.32.178.79:/apps/www/lfex



scp -i /Users/topbrids/cert/testbbs.pem /Users/topbrids/Desktop/mbm/index.html root@101.32.178.79:/apps/www/mbm


# scp -i /Users/topbrids/cert/testbbs.pem root@101.32.178.79:/apps/www/mbm ./
lib/0.4.0/ta-lib-0.4.0-src.tar.gz