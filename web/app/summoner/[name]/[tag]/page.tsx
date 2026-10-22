import { useParams } from "next/navigation";

export default function Page() {
    const { name:name, tag:tag } = useParams(); //to ci zbiera name i
    // tag tzn ktos wchodzi na naszastorna/summoner/[name]/[tag] 
    // no to jakby to zbiera
   
    //postaraj sie zrobic jak najwiecej w komponenntach tzn komponent
    //  na np mecz tzn masz np div w kotrym inne divy (mecze) to rob 
    // to w komponentach bardzo latwo potem edytowac/dodawac znmiebaic etc. 
    // przyspieszy bardoz i np komponenty per podstrona tpypu dla tej podstorny 
    // beda w  folderze componnts/summoner
    return (
        <div>Strona gracza korzystasz z endpointu /api jakis tam zaraz dokumentacje sie dokonczy</div>
    );
}
