import { useState, useEffect } from "react";
import api from "../api/api";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";

const usePositions = () => {
  const [positions, setPositions] = useState([]);
  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);
  const [loading, setLoading] = useState(true);
  const [hasAccess, setHasAccess] = useState(false);

    // -------------------
  // Initialization + Auth
  // -------------------

  useEffect(() => {
    const initialize = async () => {
      setLoading(true);
      const token = localStorage.getItem("token");

      if (!token) {
        toast.error("You are not logged in.");
        setLoading(false);
        return;
      }

      try {
        const decoded = jwtDecode(token);
        const role =
          decoded?.role ||
          decoded?.[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ];

        const isSuperUser = role === "SuperUser";
        setHasAccess(isSuperUser);

        const [positionsRes, gradesRes, levelsRes] = await Promise.all([
          api.get("/positions"),
          api.get("/jobgrades"),
          api.get("/occupationallevels"),
        ]);

        setPositions(positionsRes.data);
        setJobGrades(gradesRes.data);
        setOccupationalLevels(levelsRes.data);
      } catch (error) {
        if (error.response?.status === 403) {
          toast.error("Access denied.");
        } else if (error.response?.status === 401) {
          toast.error("Unauthorized.");
        } else {
          toast.error("Failed to load data.");
        }
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, []);

  return {
    positions,
    jobGrades,
    occupationalLevels,
    loading,
    hasAccess,
  };
}
export default usePositions;