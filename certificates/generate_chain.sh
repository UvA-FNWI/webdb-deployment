#!/bin/bash

for host in `find . -mindepth 1 -maxdepth 1 -type d -printf '%f\n'`; do

    cert="$host/${host}.crt"
    ca="$host/DigiCertCA.crt"

    dest="$host/${host}_chain.pem"
    
    cat $cert > $dest
    cat $ca >> $dest
    
    echo $dest
done