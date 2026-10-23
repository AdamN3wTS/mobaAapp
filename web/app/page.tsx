import SearchComponent from "@/components/SearchComponent";

export default function Home() {
  return (
    <div className="w-screen h-screen">
      <div className="w-full h-full flex border">
        <div className="flex w-full h-full justify-center items-center">
          <SearchComponent></SearchComponent>
        </div>
      </div>
    </div>
  );
}
