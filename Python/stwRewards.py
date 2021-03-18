# -*- coding: utf-8 -*-
"""
Created on Thu Dec 10 09:53:45 2020

@author: dhrum
"""

import requests
import sys
import items

link = sys.argv[1]
authCode = sys.argv[2]


def getReward(day):
    day_mod = int(day) % 336
    if day_mod == 0:
        day_mod = 336
    return items.ItemDictonary[str(day_mod)]

h = {
    "Authorization": f"bearer {authCode}",
    "Content-Type": "application/json"
}
r = requests.post(link, headers=h, data="{}")
    
print(str(r.status_code))

if str(r.text).find('{"errorCode":"') != '-1':
    daily_feedback = str(r.text).split("notifications", 1)[1][4:].split('],"profile', 1)[0]
    day = str(daily_feedback).split('"daysLoggedIn":', 1)[1].split(',"items":[', 1)[0]
    try:
        item = str(daily_feedback).split('[{"itemType":"', 1)[1].split('","itemGuid"', 1)[0]
        amount = str(daily_feedback).split('"quantity":', 1)[1].split("}]}", 1)[0]
        
        try:
            if "}" in amount:
                amount2 = str(amount).split("},", 1)[0]
                fndr_item = str(amount).split('itemType":"', 1)[1].split('","', 1)[0]
                fndr_amount = str(amount).split('quantity":', 1)[1]
                if fndr_item == 'CardPack:cardpack_event_founders':
                    fndr_item_f = "Founder's Llama"
                elif fndr_item == 'CardPack:cardpack_bronze':
                    fndr_item_f = "Upgrade Llama (bronze)"
                else:
                    fndr_item_f = fndr_item
                    
                print(f'On day **{day}**, you received: {getReward(day)}')
                print(f'Founders rewards:{fndr_amount} {fndr_item_f}')
                print(f'Founders: {fndr_amount} {fndr_item}')
                
            else:
                print(f'On day **{day}**, you received: {getReward(day)}')
        except:
            if "}" in amount:
                amount2 = str(amount).split("},", 1)[0]
                fndr_item = str(amount).split('itemType":"', 1)[1].split('","', 1)[0]
                fndr_amount = str(amount).split('quantity":', 1)[1]
                print(f'On day **{day}**, you received: {amount2} {item}')
                print(f'Founders rewards: {fndr_amount} {fndr_item}')
                print(f'On day **{day}**, you received: {amount2} {item}')
                
            else:
                print(f'On day **{day}**, you received: {amount2} {item}')
                
            print('using backup')
                
    except:
        print("Reward already claimed!")