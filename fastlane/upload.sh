while getopts e:t:f:c: flag
do
    case "${flag}" in
        e) email=${OPTARG};;
        t) token=${OPTARG};;
        f) filename=${OPTARG};;
        c) comment=${OPTARG};;
    esac
done
echo "Email: $email";
echo "Token: $token";
echo "Filename: $filename";
echo "Comment: $comment";

echo "Uploading attachment to 
https://nordicit.atlassian.net/wiki/spaces/M5APP/rest/api/content/2491810/child/attachment..."
echo "Please wait until uploading finishes..."

CODE=$(curl -D- -w '%{http_code}' -u "$email:$token" -X POST -H 'X-Atlassian-Token: nocheck' -F "file=@$filename" -F 'minorEdit=true' -F "comment=$comment; type=text/plain; charset=utf-8" https://nordicit.atlassian.net/wiki/rest/api/content/2491810/child/attachment)
echo $CODE


