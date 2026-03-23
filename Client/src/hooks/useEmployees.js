import { useState, useEffect } from "react";
import { fetchAllEmployees } from "../api/Employee";
/**
 * Custom React hook to fetch and manage the state of all employees.
 *
 * @param {string} locationKey - A key (e.g., from `useLocation`) that triggers refetch when changed.
 * @returns {Object} Contains:
 *  - employees: Array of employee objects fetched from the server.
 *  - loading: Boolean indicating if data is being fetched.
 *  - error: String containing error message if fetch fails.
 *  - reload: Function to manually reload employees.
 */
const useEmployees = (locationKey) => {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
    /**
   * Fetches all employees from the server and updates state.
   * Handles loading state and errors.
   */
  const loadEmployees = async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await fetchAllEmployees();
      setEmployees(data);
    } catch (err) {
      setError("Failed to load employees.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadEmployees();
  }, [locationKey]);

  return { employees, loading, error, reload: loadEmployees };
};

export default useEmployees;