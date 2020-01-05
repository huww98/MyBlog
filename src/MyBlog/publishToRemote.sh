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
remoteTarget="/opt/myblog/"
server="sh.qcloud"
echo Starting publish to $server:$remoteTarget

if [ $restart = true ]; then
	echo Stoping remote service
	ssh $server sudo service myblog stop
fi

echo transfering files
rsync -rvz --delete --checksum --exclude "appsettings.*.json" "$localDir" "$server:$remoteTarget"

if [ $restart = true ]; then
	echo restarting service
	ssh $server sudo service myblog start;
fi

echo Published to remote successfully.
