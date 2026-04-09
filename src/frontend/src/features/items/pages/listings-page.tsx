import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function ListingsPage() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Listings</CardTitle>
        <CardDescription>
          Marketplace tracking lands in Phases 3 and 4 after item management is in place.
        </CardDescription>
      </CardHeader>
      <CardContent className="text-sm text-muted-foreground">
        The architecture already includes listing drafts, platform targets, and repricing entities.
      </CardContent>
    </Card>
  );
}
