#!/bin/bash
docker stop stocktickr_stocktickr_1 \
    && cd ./StockTickRApp/ \
    && docker build . -t stocktickr:latest \
    && cd .. && docker-compose up -d