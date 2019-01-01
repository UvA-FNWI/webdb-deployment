#!/bin/bash

random-string()
{
    cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w ${1:-32} | head -n 1
}

secret="very-secret"

for host in `cat hosts | grep "uva.nl"`; do

    filename="host_vars/${host}.yaml";

    if [ -f $filename ]; then
        echo "${filename} already exists, skipping";
        continue;
    fi

    echo $host
    mysql_root_pw=`echo -n "${secret}-${host}" | sha512sum | cut -c1-25`
    echo "---" > $filename
    echo "pma_blowfish: `random-string 32`" >> $filename
    echo "mysql_root_password: ${mysql_root_pw}" >> $filename
done