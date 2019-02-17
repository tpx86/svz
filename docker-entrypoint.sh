#!/bin/bash

./wait-for-it.sh redis:6379 -t 90
./wait-for-it.sh postgres:5432 -t 90
./wait-for-it.sh elasticsearch:9200 -t 90
dotnet Svz.Tool.dll setup

