#!/bin/bash
target=${1:-http://localhost:6969/json}
while true # loop forever, until ctrl+c pressed.
do
	for i in $(seq 100) # perfrom the inner command 100 times.
	do
		curl $target > /dev/null & # send out a curl request, the & indicates not to wait for the response.
	done

	wait # after 100 requests are sent out, wait for their processes to finish before the next iteration.
done
