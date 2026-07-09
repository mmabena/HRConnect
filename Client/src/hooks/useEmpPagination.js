import { useState } from "react";

const useEmpPagination = (data = [], defaultItemsPerPage = 7) => {
  const safeData = Array.isArray(data) ? data : [];

  const [activePage, setActivePage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(defaultItemsPerPage);

  const totalPages = Math.ceil(safeData.length / itemsPerPage);

  const indexOfFirstItem = (activePage - 1) * itemsPerPage;
  const indexOfLastItem = indexOfFirstItem + itemsPerPage;

  const currentItems = safeData.slice(indexOfFirstItem, indexOfLastItem);

  return {
    activePage,
    setActivePage,
    itemsPerPage,
    setItemsPerPage,
    totalPages,
    indexOfFirstItem,
    indexOfLastItem,
    currentItems,
  };
};

export default useEmpPagination;