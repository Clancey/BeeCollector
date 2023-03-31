#!/bin/bash
set -e
echo 'Running'
docker build -t beehive -f Dockerfile .