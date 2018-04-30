#!/bin/bash

restart=true

#Read parameters
while :; do
	case $1 in
		--no-restart)
			restart=false;;
		*)
			break;;
	esac
	shift
done

localDir="bin/publishToRemote/"
remoteTarget="/var/aspnetcore/myblog/"
server="qcloud"
echo Starting publish to $server:$remoteTarget

if [ $restart = true ]; then
	echo Stoping remote service
	ssh $server sudo service kestrel-myblog stop
fi

echo transfering files
rsync -rz "$localDir" "$server:$remoteTarget"

echo setting permissions
ssh $server \
"sudo chown -R www-data:www-data \"$remoteTarget\";
sudo chmod -R 775 \"$remoteTarget\";

if [ $restart = true ]; then
	echo restarting service
	sudo service kestrel-myblog start;
fi"

echo Published to remote successfully.