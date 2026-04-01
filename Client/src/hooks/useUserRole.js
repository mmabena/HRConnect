import { useEffect, useState } from "react";

const useUserRole = () => {

 const [role, setRole] = useState(null);

 useEffect(()=>{
   setRole("superuser");
 },[]);

 return role;

};

export default useUserRole;