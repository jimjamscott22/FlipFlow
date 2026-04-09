import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function ItemsPage() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>My Items</CardTitle>
        <CardDescription>
          Phase 2 will add full CRUD, search, filters, and upload-backed photo galleries.
        </CardDescription>
      </CardHeader>
      <CardContent className="text-sm text-muted-foreground">
        The route and navigation are already wired so the item module can drop in cleanly.
      </CardContent>
    </Card>
  );
}
