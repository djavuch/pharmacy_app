import { useEffect, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Check, X } from "lucide-react";
import { adminReviewApi } from "@/shared/api";
import type { AdminReviewDto, ReviewStatus } from "@/shared/types";
import { TabsContent } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { getReviewStatusBadgeClass, renderStars } from "./status-utils";
import {
  buildUrlWithQuery,
  getCurrentUrl,
  parseEnumParam,
  parsePositiveIntParam,
} from "./query-state";

const reviewSearchByOptions = ["product", "user"] as const;
type ReviewSearchBy = (typeof reviewSearchByOptions)[number];

const reviewStatusOptions = ["all", "Pending", "Approved", "Rejected"] as const;
type ReviewFilterStatus = (typeof reviewStatusOptions)[number];

const reviewSortOptions = [
  "createdat_desc",
  "createdat_asc",
  "rating_desc",
  "rating_asc",
] as const;
type ReviewSort = (typeof reviewSortOptions)[number];

const reviewSearchByLabels: Record<ReviewSearchBy, string> = {
  product: "By product",
  user: "By user",
};

const reviewStatusLabels: Record<ReviewFilterStatus, string> = {
  all: "All statuses",
  Pending: "Pending",
  Approved: "Approved",
  Rejected: "Rejected",
};

const reviewSortLabels: Record<ReviewSort, string> = {
  createdat_desc: "Newest first",
  createdat_asc: "Oldest first",
  rating_desc: "Rating high-low",
  rating_asc: "Rating low-high",
};

export function ReviewsTab({ isActive }: { isActive: boolean }) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [reviews, setReviews] = useState<AdminReviewDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionReviewId, setActionReviewId] = useState<number | null>(null);
  const [selectedReview, setSelectedReview] = useState<AdminReviewDto | null>(null);

  const [search, setSearch] = useState(() => searchParams.get("reviewsSearch") ?? "");
  const [searchBy, setSearchBy] = useState<ReviewSearchBy>(() =>
    parseEnumParam(searchParams.get("reviewsSearchBy"), reviewSearchByOptions, "product"),
  );
  const [statusFilter, setStatusFilter] = useState<"all" | ReviewStatus>(() =>
    parseEnumParam(searchParams.get("reviewsStatus"), reviewStatusOptions, "all") as ReviewFilterStatus,
  );
  const [sort, setSort] = useState<ReviewSort>(() =>
    parseEnumParam(searchParams.get("reviewsSort"), reviewSortOptions, "createdat_desc"),
  );

  const [page, setPage] = useState(() =>
    parsePositiveIntParam(searchParams.get("reviewsPage"), 1),
  );
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 15;
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    if (!isActive) {
      return;
    }

    const nextUrl = buildUrlWithQuery(pathname, searchParams, {
      tab: "reviews",
      reviewsSearch: search || undefined,
      reviewsSearchBy: searchBy === "product" ? undefined : searchBy,
      reviewsStatus: statusFilter === "all" ? undefined : statusFilter,
      reviewsSort: sort === "createdat_desc" ? undefined : sort,
      reviewsPage: page > 1 ? page : undefined,
    });
    const currentUrl = getCurrentUrl(pathname, searchParams);

    if (nextUrl !== currentUrl) {
      router.replace(nextUrl, { scroll: false });
    }
  }, [isActive, page, pathname, router, search, searchBy, searchParams, sort, statusFilter]);

  useEffect(() => {
    const fetchReviews = async () => {
      setLoading(true);
      setError(null);

      try {
        const [sortBy, direction] = sort.split("_") as [string, "asc" | "desc"];

        const result = await adminReviewApi.getAll({
          pageIndex: page,
          pageSize,
          filterOn: search ? searchBy : undefined,
          filterQuery: search || undefined,
          sortBy,
          isAscending: direction === "asc",
          status: statusFilter === "all" ? undefined : statusFilter,
        });

        setReviews(result.items ?? []);
        setTotalPages(result.totalPages ?? 1);
      } catch (err) {
        setReviews([]);
        setTotalPages(1);
        setError(err instanceof Error ? err.message : "Failed to load reviews");
      } finally {
        setLoading(false);
      }
    };

    void fetchReviews();
  }, [page, pageSize, search, searchBy, statusFilter, sort, reloadToken]);

  const refresh = () => setReloadToken((value) => value + 1);

  const handleApprove = async (reviewId: number) => {
    setActionReviewId(reviewId);
    try {
      await adminReviewApi.approve(reviewId);
      refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to approve review");
    } finally {
      setActionReviewId(null);
    }
  };

  const handleReject = async (reviewId: number) => {
    setActionReviewId(reviewId);
    try {
      await adminReviewApi.reject(reviewId);
      refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to reject review");
    } finally {
      setActionReviewId(null);
    }
  };

  return (
    <TabsContent value="reviews">
      <Card>
        <CardHeader>
          <CardTitle>Review Moderation</CardTitle>
          <CardDescription>Approve or reject customer reviews</CardDescription>

          <div className="mt-4 grid gap-3 md:grid-cols-4">
            <Input
              placeholder="Search..."
              value={search}
              onChange={(event) => {
                setSearch(event.target.value);
                setPage(1);
              }}
            />

            <Select
              value={searchBy}
              onValueChange={(value) => {
                if (!value) return;
                setSearchBy(value as "product" | "user");
                setPage(1);
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder="Search by">
                  {(value) => (value ? reviewSearchByLabels[value as ReviewSearchBy] ?? value : "Search by")}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="product">By product</SelectItem>
                <SelectItem value="user">By user</SelectItem>
              </SelectContent>
            </Select>

            <Select
              value={statusFilter}
              onValueChange={(value) => {
                if (!value) return;
                setStatusFilter(value as "all" | ReviewStatus);
                setPage(1);
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder="Status">
                  {(value) => (value ? reviewStatusLabels[value as ReviewFilterStatus] ?? value : "Status")}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                <SelectItem value="Pending">Pending</SelectItem>
                <SelectItem value="Approved">Approved</SelectItem>
                <SelectItem value="Rejected">Rejected</SelectItem>
              </SelectContent>
            </Select>

            <Select
              value={sort}
              onValueChange={(value) => {
                if (!value) return;
                setSort(value as "createdat_desc" | "createdat_asc" | "rating_desc" | "rating_asc");
                setPage(1);
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder="Sort by">
                  {(value) => (value ? reviewSortLabels[value as ReviewSort] ?? value : "Sort by")}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="createdat_desc">Newest first</SelectItem>
                <SelectItem value="createdat_asc">Oldest first</SelectItem>
                <SelectItem value="rating_desc">Rating high-low</SelectItem>
                <SelectItem value="rating_asc">Rating low-high</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>

        <CardContent>
          {error && (
            <div className="mb-3 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
              {error}
            </div>
          )}

          {loading ? (
            <p className="py-8 text-center text-muted-foreground">Loading...</p>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>Product</TableHead>
                    <TableHead>User</TableHead>
                    <TableHead>Rating</TableHead>
                    <TableHead>Comment</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {reviews.map((review) => (
                    <TableRow key={review.id}>
                      <TableCell>{review.id}</TableCell>
                      <TableCell>
                        <div className="font-medium">{review.productName || "Unknown product"}</div>
                        <div className="text-xs text-muted-foreground">ID: {review.productId}</div>
                      </TableCell>
                      <TableCell>{review.userName || review.userId || "Unknown user"}</TableCell>
                      <TableCell>
                        <div className="inline-flex items-center gap-0.5">
                          {renderStars(review.rating)}
                        </div>
                      </TableCell>
                      <TableCell className="max-w-xs">
                        <p className="line-clamp-2 text-sm text-muted-foreground">
                          {review.content?.trim() || "No comment"}
                        </p>
                      </TableCell>
                      <TableCell>
                        {new Date(review.createdAt).toLocaleDateString("en-US")}
                      </TableCell>
                      <TableCell>
                        <Badge className={getReviewStatusBadgeClass(review.status)}>
                          {review.status}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setSelectedReview(review)}
                          >
                            View
                          </Button>
                          {review.status !== "Approved" && (
                            <Button
                              variant="outline"
                              size="sm"
                              disabled={actionReviewId === review.id}
                              onClick={() => void handleApprove(review.id)}
                            >
                              <Check className="mr-1 h-3.5 w-3.5" />
                              Approve
                            </Button>
                          )}
                          {review.status !== "Rejected" && (
                            <Button
                              variant="outline"
                              size="sm"
                              disabled={actionReviewId === review.id}
                              onClick={() => void handleReject(review.id)}
                            >
                              <X className="mr-1 h-3.5 w-3.5" />
                              Reject
                            </Button>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {reviews.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">
                  No reviews found for current filters.
                </p>
              )}

              {totalPages > 1 && (
                <div className="mt-4 flex items-center justify-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => setPage((value) => value - 1)}
                  >
                    Previous
                  </Button>
                  <span className="text-sm text-muted-foreground">
                    {page} / {totalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page >= totalPages}
                    onClick={() => setPage((value) => value + 1)}
                  >
                    Next
                  </Button>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog
        open={Boolean(selectedReview)}
        onOpenChange={(isOpen) => {
          if (!isOpen) setSelectedReview(null);
        }}
      >
        <DialogContent className="max-w-xl">
          <DialogHeader>
            <DialogTitle>Review details</DialogTitle>
            <DialogDescription>
              {selectedReview?.productName || "Unknown product"} by{" "}
              {selectedReview?.userName || selectedReview?.userId || "Unknown user"}
            </DialogDescription>
          </DialogHeader>

          {selectedReview && (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <div className="inline-flex items-center gap-0.5">
                  {renderStars(selectedReview.rating)}
                </div>
                <Badge className={getReviewStatusBadgeClass(selectedReview.status)}>
                  {selectedReview.status}
                </Badge>
              </div>
              <div className="rounded-md border bg-muted/30 p-3 text-sm text-muted-foreground">
                {selectedReview.content?.trim() || "No comment"}
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </TabsContent>
  );
}
