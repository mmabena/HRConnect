import { useEffect, useState } from "react";
import { getStoredUserRole } from "../utils/roleUtils";

const useUserRole = () => {
  const [role, setRole] = useState(null);

  useEffect(() => {
    setRole(getStoredUserRole().key);
  }, []);

  return role;

};

export default useUserRole;