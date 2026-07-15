import { useState, useEffect } from "react";
import { fetchMyCompanies } from "../api/UserCompany";

const useUserCompanies = () => {
  const [companies, setCompanies] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const loadCompanies = async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await fetchMyCompanies();
      setCompanies(data);
    } catch (err) {
      setError("Failed to load employees.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCompanies();
  }, []);

  return {
    companies,
    loading,
    error,
    reload: loadCompanies,
  };
};

export default useUserCompanies;
