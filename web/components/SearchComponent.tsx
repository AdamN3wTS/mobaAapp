"use client"

import { useState } from "react"
import { GiCrossedSwords } from "react-icons/gi";
import { FiSearch } from "react-icons/fi";
import Link from "next/link";
export default function SearchComponent(){
    const [name,setName] = useState("");
    const [tag,setTag] = useState("");

    return(<div className="w-160 h-18 flex rounded-full  bg-white overflow-hidden">
        <div className="w-6/11 py-3 px-5  h-full flex ">
            <input type="text" className="w-full" placeholder={"Game name"} value={name} onChange={(e)=>{setName(e.currentTarget.value)}}/>
        </div>

         <div className="w-3/11 h-full flex py-3 px-5 border-l-1 border-neutral-200">
            <input type="text" className="w-full" placeholder={"Tag"}  value={tag} onChange={(e)=>{setTag(e.currentTarget.value)}}/>

        </div>
         <div className="w-2/11 h-full flex justify-center items-center ">
            <Link href={`/summoner/${name}/${tag}`}>
                <FiSearch size={34} className="text-bold text-emerald-300"/>
            </Link>
        </div>
    </div>)
}