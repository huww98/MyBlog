param (
    [switch]$norestart
);

$localDir="bin/publishToRemote/"
dotnet publish --configuration Release --output "$localDir"
if(!$?)
{
	echo "dotnet publish failed. Abort."
	exit 1
}

$bashParams=,"publishToRemote.sh"
if($norestart)
{
	$bashParams += "--no-restart"
}
& bash $bashParams
