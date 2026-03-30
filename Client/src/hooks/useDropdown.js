import { useState } from "react";

const useDropdown = () => {
  const [dropdownOpen, setDropdownOpen] = useState(false);

  const toggleDropdown = () => {
    setDropdownOpen((prev) => !prev);
  };

  const closeDropdown = () => {
    setDropdownOpen(false);
  };

  return {
    dropdownOpen,
    toggleDropdown,
    closeDropdown,
  };
};

export default useDropdown;