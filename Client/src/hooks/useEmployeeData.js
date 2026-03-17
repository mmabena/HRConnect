import { useEffect, useState } from "react";
import axios from "axios";
import { fetchAllEmployees } from "../Employee";

/**
 * Custom React hook responsible for retrieving employee
 * and position data from the API when a component mounts.
 *
 * @returns {Object} An object containing:
 * - positions: list of available job positions
 * - allEmployees: list of all employees
 * - loading: indicates whether the data is still being fetched
 */
const useEmployeeData = () => {

  const [positions, setPositions] = useState([]);
  const [allEmployees, setAllEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  /**
   * Runs once when the component using this hook mounts.
   * Fetches employees and positions from the backend.
   */
  useEffect(() => {

    const fetchData = async () => {
      try {

        const employees = await fetchAllEmployees();
        const positionsRes = await axios.get(
          "http://localhost:5147/api/positions"
        );

        setAllEmployees(employees);
        setPositions(positionsRes.data);

      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();

  }, []);

  return {
    positions,
    allEmployees,
    loading,
  };
};

export default useEmployeeData;