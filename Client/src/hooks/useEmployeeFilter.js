import { useMemo } from "react";

const useEmployeeFilter = (employees, selectedTab, searchQuery) => {
  const filteredEmployees = useMemo(() => {
    return employees.filter((emp) => {
      if (selectedTab !== "All") {
        const empDepartment = (emp.branch || "")
          .toLowerCase()
          .replace(/\s+/g, "");

        const selected = selectedTab.toLowerCase().replace(/\s+/g, "");

        if (empDepartment !== selected) {
          return false;
        }
      }

      const search = searchQuery.toLowerCase();
      if (!search) return true;

      const fullName = `${emp.name} ${emp.surname}`.toLowerCase();
      const jobTitle = emp.positionTitle?.toLowerCase() || "";
      const email = emp.email?.toLowerCase() || "";
      const id = emp.employeeId?.toString() || "";

      return (
        fullName.includes(search) ||
        jobTitle.includes(search) ||
        email.includes(search) ||
        id.includes(search)
      );
    });
  }, [employees, selectedTab, searchQuery]);

  return filteredEmployees;
};

export default useEmployeeFilter;