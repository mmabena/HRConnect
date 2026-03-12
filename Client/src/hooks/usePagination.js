import { useState } from "react";

const usePagination = (data, defaultItemsPerPage = 7) => {
  const [activePage, setActivePage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(defaultItemsPerPage);

  const totalPages = Math.ceil(data.length / itemsPerPage);

  const indexOfFirstItem = (activePage - 1) * itemsPerPage;
  const indexOfLastItem = indexOfFirstItem + itemsPerPage;

  const currentItems = data.slice(indexOfFirstItem, indexOfLastItem);

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

export default usePagination;