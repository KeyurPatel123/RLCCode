import { Injectable } from '@angular/core';

@Injectable()
export class StorageService {
    isClient = false;

    constructor() {
        if (typeof window !== 'undefined')
        {
            this.isClient = true;
        }
    }    

    public SessionSetItem(key: string, data:string)
    {
        if (this.isClient) {
            sessionStorage.setItem(key, data);
        }
    }

    public SessionGetItem(key: string) : string
    {
        var data = "";
        if (this.isClient) {
            data = sessionStorage.getItem(key);
        }
        return data;
    }    
}